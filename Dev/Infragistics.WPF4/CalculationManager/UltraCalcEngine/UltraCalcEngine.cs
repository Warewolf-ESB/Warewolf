// AS 9/9/04
// Temporary flag to prevent deferred calculations.
//
//#define AllowDeferredCalculations

// MRS 2/4/2008 - BR30242
//#define DEBUG_OUTPUT_TOPOLOGY_INFO

using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;
using System.Security;


using UltraCalcFunction = Infragistics.Calculations.Engine.CalculationFunction;
using UltraCalcNumberStack = Infragistics.Calculations.Engine.CalculationNumberStack;
using UltraCalcValue = Infragistics.Calculations.Engine.CalculationValue;
using IUltraCalcReference = Infragistics.Calculations.Engine.ICalculationReference;
using IUltraCalcFormula = Infragistics.Calculations.Engine.ICalculationFormula;
using UltraCalcErrorValue = Infragistics.Calculations.Engine.CalculationErrorValue;
using IUltraCalcReferenceCollection = Infragistics.Calculations.Engine.ICalculationReferenceCollection;
using UltraCalcErrorCode = Infragistics.Calculations.Engine.CalculationErrorCode;
using UltraCalcReferenceError = Infragistics.Calculations.Engine.CalculationReferenceError;
using UltraCalcException = Infragistics.Calculations.Engine.CalculationException;
using UltraCalcErrorException = Infragistics.Calculations.Engine.CalcErrorException;
using UltraCalcNumberException = Infragistics.Calculations.Engine.CalcNumberException;
using UltraCalcValueException = Infragistics.Calculations.Engine.CalculationValueException;
using UltraCalcAction = Infragistics.Calculations.Engine.CalculationEngineAction;




using iwu = Infragistics.CoreUtilities;
namespace Infragistics.Calculations.Engine









{
	#region UltraCalcEngine Class



#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)

	internal class UltraCalcEngine 

		: ISupportPropertyChangeNotifications

	{
		#region Member Variables

		// Instance of the UltraCalcAncestorMap used to lookup ancestors of a given reference
		private UltraCalcAncestorMap ancestorMap;

		// Instance of the UCRecalcChange used to track dirty references whose formulas need to be recalculated
		private UCRecalcChain recalcChain;

		// If non-null, it contains a reference within a circular reference cycle
		private IUltraCalcReference circularReference;

		// Denotes the time interval used in time-based recalculation cycles
		private const long RecalcTaskTicks = 2 * 100000;

		// Function factory containing builtin and user defined functions registered with the calc engine instance
		private UltraCalcFunctionFactory functionFactory;

		// Denotes that the recalc chain needs to be sorted due to formulas being dirtied
		private bool doTopologicalSort;

		// A list of references whose formulas contain always dirty functions
		private UltraCalcReferenceCollection alwaysDirtyList;

		// Event queue used to process topological events
        private Queue<QueueInfo> eventQueue;

		// SSP 10/1/04
		// Commented out the following three variables. Instead added RangeRecalcInfo
		// instance firther down that holds all the necessary info regarding range recalc.
		// 
		//private IUltraCalcReference pendingRangeCalcReference;
		//private IEnumerator pendingRangeCalcReferenceEnumerator;
		//private IUltraCalcFormula pendingRangeCalcReferenceFormula;
		private bool enableMarkedColumns; 
		private bool topoSortEventEnabled;
		private bool topoCreateEventEnabled;
		private bool topoInsertEventEnabled;
		private bool topoDeleteEventEnabled; 
		private bool topoRemoveEventEnabled;
		private bool topoResyncEventEnabled;
		private bool topoVisibleEventEnabled; 

		// SSP 9/30/04 - Circular Relative Index Support & Optimizations
		//
		#region Circular Relative Index Support & Optimizations

		private int ancestorMapVersion = 0;
		private int verifiedAncestorMapVersion = -1;

		// MD 8/22/08 - Excel formula solving - Performance
		internal RangeCalcInfo pendingCalcInfo = null;		 
		private bool removeDisposedReferencesFromRecalcChain = false;

		// SSP 10/15/04
		// Added a way to suspend processing of calc notifications because if there are a lot of
		// such notifications being sent from the grid then it may take a very long time to process
		// them. Workaround would be to suspend notification processing and then dirty all the 
		// formulas so every thing gets recalculated.
		//
		private int suspendNotificationProcessingCounter = 0;

		#endregion // Circular Relative Index Support & Optimizations

		// MD 8/12/08 - Excel formula solving
		// Added an anti-recursion flag to make sure we don't try to evaluate something on the recalc chain recursively.
		private bool isEvaluating;

		// MD 8/20/08 - Excel formula solving
		private bool allowCircularities;
		private Dictionary<IUltraCalcReference, object> circularReferences = new Dictionary<IUltraCalcReference, object>();
		private bool iterativeCalculationsEnabled;
		private double maxChangeInIteration;
		private int maxRecursionIterations;

		// MD 11/1/10 - TFS56976
		// Keep a flag which determines when we can skip the dirtying of parents.
		private bool preventDirtyParents;

		#endregion // Member Variables

		#region QueueInfo nested class

		internal class QueueInfo
		{
			private IUltraCalcReference reference;
			private IUltraCalcFormula formula;
			private ReferenceActionCode actionCode;

			#region Constructor
			internal QueueInfo(IUltraCalcReference reference, ReferenceActionCode actionCode) : this(reference, actionCode, null)
			{
			}

			internal QueueInfo(IUltraCalcReference reference, ReferenceActionCode actionCode, IUltraCalcFormula formula)
			{
				this.reference = reference;
				this.formula = formula;
				this.actionCode = actionCode;
			}
			#endregion //Constructor

			internal IUltraCalcReference Reference
			{
				get { return this.reference; }
			}

			internal IUltraCalcFormula Formula
			{
				get { return this.formula; }
			}

			internal ReferenceActionCode Action
			{
				get { return this.actionCode; }
			}
		}

		#endregion // QueueInfo

		#region Constructor

		/// <summary>
		/// Initializes a new <see cref="UltraCalcEngine"/>
		/// </summary>
		public UltraCalcEngine()
		{
			this.ancestorMap = new UltraCalcAncestorMap(this);
			this.recalcChain = new UCRecalcChain(this);

			//Ticks = 100 * 100000;
			this.functionFactory = new UltraCalcFunctionFactory();
			this.enableMarkedColumns = true; 
			this.doTopologicalSort = false;
			this.alwaysDirtyList = new UltraCalcReferenceCollection();
			this.eventQueue = new Queue<QueueInfo>();
			this.topoSortEventEnabled = true; 
			this.topoCreateEventEnabled = true;
			this.topoInsertEventEnabled = true;
			this.topoDeleteEventEnabled = true; 
			this.topoRemoveEventEnabled = true;
			this.topoResyncEventEnabled = true;
			this.topoVisibleEventEnabled = true; 
		}

		#endregion //Constructor

		#region Events

		#region Event Declarations



#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		#endregion //Event Declarations 

		#region OnXXX event methods

		/// <summary>
		/// Used to invoke the <see cref="RecalcValue"/> event
		/// </summary>
		/// <param name="reference">Reference</param>
		// SSP 7/13/11 - XamCalculationManager
		// 
		//protected virtual void OnRecalcValue(UltraCalcReferenceEventArgs e)
		protected virtual void OnRecalcValue( IUltraCalcReference reference )
		{

			_listeners.OnPropertyValueChanged( this, "RecalcValue", reference );


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		}

		/// <summary>
		/// Used to invoke the <see cref="Circularity"/> event
		/// </summary>
		/// <param name="reference">Reference</param>
		// SSP 7/13/11 - XamCalculationManager
		// 
		//protected virtual void OnCircularity( UltraCalcReferenceEventArgs e )
		protected virtual void OnCircularity( IUltraCalcReference reference )
		{

			_listeners.OnPropertyValueChanged( this, "Circularity", reference );


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		}

		/// <summary>
		/// Used to invoke the <see cref="DirtyValue"/> event
		/// </summary>
		/// <param name="reference">Reference</param>
		// SSP 7/13/11 - XamCalculationManager
		// 
		//protected virtual void OnDirtyValue( UltraCalcReferenceEventArgs e )
		protected virtual void OnDirtyValue( IUltraCalcReference reference )
		{

			_listeners.OnPropertyValueChanged( this, "DirtyValue", reference );


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		}

		#endregion //OnXXX event methods

		#endregion //Events

		#region Public Properties

		#region EnableMarkedColumns

		/// <summary>
		/// Returns or sets whether deferred calculations are enabled.
		/// </summary>
		public bool EnableMarkedColumns
		{
			get
			{
				return this.enableMarkedColumns;
			}
			set
			{
				if ( this.enableMarkedColumns != value )
				{
					this.enableMarkedColumns = value;

					// Bump the ancestor map version number so RecalcDeferred will be reinitialized on
					// all the formula references.
					//
					this.BumpAncestorMapVersion();
				}
			}
		}

		#endregion //EnableMarkedColumns

		#region FunctionFactory

		/// <summary>
		/// Returns the formula function factory for this instance of the calc engine
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Use the function factory to access detailed information about each function available to formulas compiled against this instance of the calc engine.
		/// </p>
		/// </remarks>
		public UltraCalcFunctionFactory FunctionFactory
		{
			get { return functionFactory; }
		}

		#endregion // FunctionFactory

		#endregion // Public Properties

		#region Private/Internal Properties

		#region EventQueue






        internal Queue<QueueInfo> EventQueue
		{
			get
			{
				return this.eventQueue;
			}
		}

		#endregion // EventQueue 

		#region HasMoreToCalc
		






		private bool HasMoreToCalc
		{
			get
			{
				// MD 8/22/08 - Excel formula solving - Performance
				// Refactored so the check for the pendingCalcInfo can be ifdef-ed out.
				//return ( null != this.pendingCalcInfo || this.recalcChain.Count > 0 )
				//    // MD 8/20/08 - Excel formula solving
				//    // If there is a cicular reference formula but circularities are allowed, calculations can continue
				//    //&& null == this.circularReference;
				//    && ( null == this.circularReference || this.AllowCircularities );
				if ( this.circularReference != null && this.AllowCircularities == false )
					return false;

				if ( this.recalcChain.Count > 0 )
					return true;

				if ( null != this.pendingCalcInfo )
					return true; 

				return false;
			}
		}

		#endregion // HasMoreToCalc

		#region AncestorMap
		





		private  UltraCalcAncestorMap AncestorMap
		{
			get { return ancestorMap; }
		}

		#endregion //AncestorMap

		#region CircularReference

		/// <summary>
		/// Gets or sets a reference within a circular reference cycle.
		/// </summary>
		public IUltraCalcReference CircularReference
		{
			get { return circularReference; }
		}

		#endregion //CircularReference 

		#region IsNotificationProcessingSuspended






		internal bool IsNotificationProcessingSuspended
		{
			get
			{
				return this.suspendNotificationProcessingCounter > 0;
			}
		}

		#endregion // IsNotificationProcessingSuspended 

		// MD 8/20/08 - Excel formula solving
		#region AllowCircularities






		internal bool AllowCircularities
		{
			get { return this.allowCircularities; }
			set { this.allowCircularities = value; }
		} 

		#endregion AllowCircularities

		#region IterativeCalculationsEnabled






		internal bool IterativeCalculationsEnabled
		{
			get { return this.iterativeCalculationsEnabled; }
			set { this.iterativeCalculationsEnabled = value; }
		} 

		#endregion IterativeCalculationsEnabled

		#region MaxChangeInIteration



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal double MaxChangeInIteration
		{
			get { return this.maxChangeInIteration; }
			set { this.maxChangeInIteration = value; }
		} 

		#endregion MaxChangeInIteration

		#region MaxRecursionIterations






		internal int MaxRecursionIterations
		{
			get { return this.maxRecursionIterations; }
			set { this.maxRecursionIterations = value; }
		} 

		#endregion MaxRecursionIterations

		#endregion // Private/Internal Properties

		#region Private/Internal Methods

		#region ProcessAddFormula
		


#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

		private void ProcessAddFormula(IUltraCalcFormula formula, IUltraCalcReference baseReference)
		{
			// AS 9/17/04
			// See if the reference is already in the ancestor chain.
			FormulaReference previousAncestorReference = this.ancestorMap.FindAncestor(baseReference);

			// if we find a matching item already in the ancestor map...
			if (previousAncestorReference != null)
			{
				// if the formula instance matches, exit
				if (previousAncestorReference.Formula == formula)
					return;

				// if the formula is different, remove the old one
				this.ProcessDeleteFormula(previousAncestorReference.Formula, previousAncestorReference.UnderlyingReference);
			}

			// AS 9/17/04
			// If the formula has since changed, do not add it.
			//
			if (baseReference.Formula != formula)
				return;

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
				alwaysDirtyList.Add( new FormulaReference(baseReference) );

			// Add object to the dirty chain
			DirtyParents(baseReference, true);

			this.BumpAncestorMapVersion( );
		}

		#endregion // ProcessAddFormula

		#region FormulasContainedBy



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		private UltraCalcReferenceCollection FormulasContainedBy(IUltraCalcReference baseReference, IUltraCalcReferenceCollection referenceCollection)
		{
			UltraCalcReferenceCollection referenceList = new UltraCalcReferenceCollection();

			foreach (IUltraCalcReference reference in referenceCollection)
			{
				// unwrap any ancestor references so we never call
				// the reference's contains reference with the wrapped object
				IUltraCalcReference referenceResolved = reference is FormulaReference ? ((FormulaReference)reference).UnderlyingReference : reference;

				if (baseReference.ContainsReference(referenceResolved))
					referenceList.Add(reference);
			}

			return referenceList;
		}

		#endregion // FormulasContainedBy

		#region FormulasSubsetOf
		


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		private UltraCalcReferenceCollection FormulasSubsetOf(IUltraCalcReference baseReference, IUltraCalcReferenceCollection referenceCollection)
		{
			UltraCalcReferenceCollection referenceList = new UltraCalcReferenceCollection();
			foreach (IUltraCalcReference reference in referenceCollection)
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
				IUltraCalcReference referenceResolved = reference is FormulaReference ? ((FormulaReference)reference).UnderlyingReference : reference;

				if ( baseReference.IsSubsetReference( referenceResolved ) )
					referenceList.Add( reference );
				// --------------------------------------------------------------------------------------
			}

			return referenceList;
		}

		#endregion //FormulasSubsetOf

		#region ProcessDeleteFormula
		


#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

		private void ProcessDeleteFormula(IUltraCalcFormula formula, IUltraCalcReference baseReference)
		{
			// SSP 10/7/04
			// Commented out the following code and added the necessary code at the end of this method.
			//
			
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


			bool noReferences = false;
			// Remove the formula from each of its predecessor's ancestor list
			foreach (IUltraCalcReference reference in formula.References) 
			{
				ancestorMap.DeleteAncestor(reference, baseReference);
				noReferences = true;
			}

			// Calculated reference without RHS references are made ancestors of a dummy entry so they can be found when needed.
			if (!noReferences) 
			{
				// Remove from this list.
				UCReference reference = new UCReference(".");
				ancestorMap.DeleteAncestor(reference, baseReference);
			}


			// Remove any always dirty reference
			if (formula.HasAlwaysDirty) 
			{
				// SSP 10/7/04
				// Commented this code out and added the necessary code below.
				//
				
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


				alwaysDirtyList.Remove(baseReference);
			}

			// SSP 10/7/04
			// Remove the reference and any subset references (column reference as well as cells belonging to it).
			//
			this.recalcChain.RemoveSubsetReferences( baseReference );

			// Dirty the formula object's parents
			DirtyParents(baseReference, false);

			// Bump the acnestor map version number so we can reassign the formula sort numbers and other
			// info to all the formulas.
			//
			this.BumpAncestorMapVersion( );
		}

		#endregion //ProcessDeleteFormula

		#region DisposeReferences

		private void DisposeReferences()
		{
			// Remove any disposed references on recalc chain
			recalcChain.RemoveDisposedReferences();

			// Disconnect any disposed predecessors and remove any disposed formulas from ancestor map
			UltraCalcReferenceCollection disconnectedReferences = ancestorMap.DisposeReferences();

			// Dirty any disconnected references
			foreach ( IUltraCalcReference reference in disconnectedReferences )
				DirtyParents( reference, false );

			// Clear pending recalc
			//ClearPendingRangeCalc();

		}

		#endregion //DisposeReferences 

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
		private void AddFormulas( IUltraCalcReference addedReference )
		{
			if (null != addedReference && !addedReference.IsDisposedReference)
			{
				IUltraCalcFormula formula = addedReference.Formula;

				// We shouldn't add the formula for cell references. The way we can check this
				// is by making sure that the base reference of the formula is the same as
				// the reference we are processing.
				//
				if (null != formula && formula.BaseReference == addedReference && !formula.HasSyntaxError)
				{
					this.ProcessAddFormula( formula, formula.BaseReference );
				}

				IUltraCalcReference[] childReferences = addedReference.GetChildReferences( ChildReferenceType.ReferencesWithFormulas );
				if ( null != childReferences )
				{
					foreach ( IUltraCalcReference reference in childReferences )
						this.AddFormulas( reference );
				} 
			}
		}

		#endregion //AddFormulas

		#region ProcessTopologicalEvent







		private void ProcessTopologicalEvent( QueueInfo item )
		{
            // MRS 2/4/2008 - BR30242
            this.DebugOutputProcessQueueItem(item);

			if ( item.Action == ReferenceActionCode.Sort )
			{
				// Connect all references contained by the event reference
				if ( topoSortEventEnabled )
					SortReferences( item.Reference );
			}
			else if (item.Action == ReferenceActionCode.Create)
			{	
				// Connect all references contained by the event reference
				if (topoCreateEventEnabled)
				{
					ConnectReferences(item.Reference, 1);

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
					this.AddFormulas( item.Reference );
				}
			}
			else if ( item.Action == ReferenceActionCode.Insert )
			{
				// Connect all references contained by the event reference
				if ( topoInsertEventEnabled )
					InsertReferences( item.Reference, true );
			}
			else if ( item.Action == ReferenceActionCode.Delete )
			{
				// Disconnect all references contained by the event reference
				if ( topoDeleteEventEnabled )
					InsertReferences( item.Reference, false );
			} 
			else if (item.Action == ReferenceActionCode.Remove)
			{
				if (topoRemoveEventEnabled)
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
						foreach (IUltraCalcReference reference in this.FormulasSubsetOf(item.Reference, entry.Ancestors))
						{
							if (references.IndexOf(reference) == -1)
								references.Add(reference);							
						}
					}

					foreach (IUltraCalcReference reference in references)
					{
						ProcessDeleteFormula(reference.Formula, ((FormulaReference)reference).UnderlyingReference);
					}
			
					// Disconnect all references contained by the event reference
					ConnectReferences(item.Reference, 0);
				}
			}
			else if ( item.Action == ReferenceActionCode.Resync )
			{
				// Connect all references contained by the event reference
				if ( topoResyncEventEnabled )
				{
					DisposeReferences();

					ConnectReferences( item.Reference, -1 );
				}
			}
			else if ( item.Action == ReferenceActionCode.Visible )
			{
				Debug.Assert( item.Reference.RecalcDeferred, "Visible event is valid only on references that are recalc deferred." );

                if (topoVisibleEventEnabled)
                {
                    // MRS 6/6/2012 - TFS113551
                    // This fix is causing a new problem.
                    // We are getting stuck in an infinite loop where every time the grid paints
                    // it dirties the summary formula(s), which causes the grid to dirty child elements which causes
                    // the grid to paint, etc. 
                    // Also, this fix was never ideal, because we are still using a lot of CPU time and loading
                    // all of the data in the grid. We are just breaking it up into chunks instead of doing it
                    // all at once, but it's mostly unneccessary and using up a lot of CPU time.
                    // So I reverted back to the original code and fixed TFS108281 in a different way (in the 
                    // WinGrid assembly) by changing the iterator to only get the visible summary values via
                    // UIElements instead of loop through the grid rows. 
                    //
                    //// MRS 4/10/2012 - TFS108281
                    //// Forcing the dirtying of all the child references caused a performance
                    //// problem, because the iterator was forcing all of the rows to get loaded
                    //// since Summaries are not deferred and the DataSet was really slow loading
                    //// all the rows. 
                    //// By calling DirtyParents only on the references itself, we essentially
                    //// push off the iteration of the reference until the Calculation time, which
                    //// is done asynchronously, so it doesnt' lock up the UI. 
                    ////
                    ////RecalcReferences(item.Reference);
                    //this.DirtyParents(item.Reference, true);
                    RecalcReferences(item.Reference);
                }
			} 
			else if (item.Action == ReferenceActionCode.AddFormula)
			{
				ProcessAddFormula(item.Formula, item.Reference);
			}
			else if (item.Action == ReferenceActionCode.DeleteFormula)
			{
				ProcessDeleteFormula(item.Formula, item.Reference);
			}
		}

		#endregion // ProcessTopologicalEvent

		#region CleanTopologicalEventQueue
		






		// MD 8/20/08 - Excel formula solving
		// Made internal
		//private bool CleanTopologicalEventQueue( long ticks )
		internal bool CleanTopologicalEventQueue( long ticks )
		{
			// MD 11/1/10 - TFS56976
			// If the ancestor map is empty when we are about to process events, we don't need to directy parents when reference values change 
			// because all formulas which are going to be added here are going into the reclac chain automatically.
			this.preventDirtyParents = (this.ancestorMap.Count == 0);

			QueueInfo item;

			// This is for not performing DisposeReferences operation more than once 
			// while processing the event queue.
			//
			int performedDisposeReferences_QueueCount = -1;

			long startTicks = DateTime.Now.Ticks;
			while ( eventQueue.Count > 0 && ( -1 == ticks || DateTime.Now.Ticks - startTicks < ticks ) )
			{
				item = (QueueInfo)eventQueue.Dequeue();
				performedDisposeReferences_QueueCount--;

				if ( item.Reference.IsDisposedReference )
				{
					// This is for not performing DisposeReferences operation more than once 
					// while processing the event queue.
					//
					if ( performedDisposeReferences_QueueCount != this.eventQueue.Count )
					{
						DisposeReferences();
						performedDisposeReferences_QueueCount = this.eventQueue.Count;
					}
				}
				else 
				{
					this.ProcessTopologicalEvent( item );
				}
			}

			if ( this.eventQueue.Count > 0 )
				return false;

			// SSP 10/15/04
			// Instead of removing the disposed references synchronously every time a row is deleted,
			// delete disposed references once when we get called Recalc the next time.
			//
			if ( this.removeDisposedReferencesFromRecalcChain )
			{
				this.recalcChain.RemoveDisposedReferences();
				this.removeDisposedReferencesFromRecalcChain = false;
			} 

			this.VerifyAncestorMap( );
			return true;
		}

		#endregion //CleanTopologicalEventQueue

		#region RecalcReferences



#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

		private void RecalcReferences( IUltraCalcReference reference )
		{
			foreach ( IUltraCalcReference cellReference in reference.References )
				// Dirty reference if it needs to be recalculated
				if ( cellReference.RecalcVisible )
					DirtyParents( cellReference, true );
		}

		#endregion //RecalcReferences 

		#region InsertReferences



#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)

		private void InsertReferences( IUltraCalcReference reference, bool insert )
		{
			foreach ( UltraCalcAncestorMap.AncestorMapEntry predecessorEntry in AncestorMap )
			{
				// Connect or disconnect references contained by the given reference and recalc any ancestors of these references
				if ( predecessorEntry.Predecessor.IsConnected && reference.ContainsReference( predecessorEntry.Predecessor.Reference ) )
				{
					// Reconnect absolute reference?
					if ( predecessorEntry.Predecessor.HasAbsoluteIndex )
						ConnectAncestorReferences( predecessorEntry.Predecessor, predecessorEntry.Ancestors, -1 ); 

					// Resolve predecessor against the event reference to obtain the specific predecessor that is being created and dirty its ancestors
					DirtyParents( reference.ResolveReference( predecessorEntry.Predecessor.Reference, ResolveReferenceType.RightHandSide ), false );
				}
			}

			if ( insert )
			{
				// Put any calculated columns for the row on the recalc chain if there not already on it
				foreach ( UltraCalcAncestorMap.AncestorMapEntry entry in ancestorMap )
					foreach ( IUltraCalcReference formulaReference in FormulasContainedBy( reference, entry.Ancestors ) )
					{
						// unwrap the reference first
						IUltraCalcReference formulaRefResolved = ( (FormulaReference)formulaReference ).UnderlyingReference;

						// AS 9/17/04
						// If the reference in the ancestor map had a formula but the formula
						// on the reference has since been changed, skip this reference.
						if ( formulaRefResolved.Formula != formulaReference.Formula )
							continue;

						IUltraCalcReference convReference = reference.ResolveReference( formulaRefResolved, ResolveReferenceType.LeftHandSide );

						// SSP 10/6/04
						// Got rid of RecalcListed, RecalcDirty and RecalcMarked flags.
						//
						//if (!convReference.RecalcDirty)
						if ( !this.recalcChain.IsRecalcListed( convReference ) )
							DirtyParents( convReference, true );
					}
			}
			else
			{
				// SSP 10/15/04
				// Instead of removing the disposed references synchronously every time a row is deleted,
				// delete disposed references once when we get called Recalc the next time.
				//
				this.removeDisposedReferencesFromRecalcChain = true;
				
#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)

			}
		}

		#endregion //InsertReferences 

		#region ConnectReferenceHelper

		// SSP 7/7/05
		// Added ConnectReferenceHelper method. Code in there is moved from the the existing 
		// ConnectAncestorReferences method.
		// 
		private void ConnectReferenceHelper( int connect, UCReference reference, IUltraCalcReference baseReference )
		{
			IUltraCalcReference baseReferenceResolved = baseReference is FormulaReference ? ((FormulaReference)baseReference).UnderlyingReference : baseReference;

			bool referenceChanged = false;
			if (connect == 1)
				referenceChanged = reference.Connect( baseReferenceResolved );
			else if (connect == 0)
				referenceChanged = reference.Disconnect();
			else if (connect == -1)
				referenceChanged = reference.Reconnect( baseReferenceResolved );

			// If the UCReference changed then bump the ancestor map version number.
			//
			if ( referenceChanged )
				this.BumpAncestorMapVersion( );
		}
		
		#endregion // ConnectReferenceHelper

		#region ConnectAncestorReferences
		


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		private void ConnectAncestorReferences(UCReference predecessor, UltraCalcReferenceCollection ancestors, int connect)
		{
			// SSP 7/7/05
			//
			IUltraCalcReference predecessorBaseReference = null;

			foreach (IUltraCalcReference ancestor in ancestors)
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
					if ( ancestorReference == predecessor )
						predecessorBaseReference = ancestor;
					else if ( UltraCalcEngine.CompareReferences( ancestorReference, predecessor ) )
						this.ConnectReferenceHelper( connect, ancestorReference, ancestor );
					
#region Infragistics Source Cleanup (Region)















#endregion // Infragistics Source Cleanup (Region)

					// --------------------------------------------------------------------------------------
				}
			}

			// SSP 7/7/05
			// Related to the change above in the for loop.
			// 
			if ( null != predecessorBaseReference )
				this.ConnectReferenceHelper( connect, predecessor, predecessorBaseReference );
		}

		#endregion //ConnectAncestorReferences

		#region ConnectReferences
		


#region Infragistics Source Cleanup (Region)















#endregion // Infragistics Source Cleanup (Region)

		private void ConnectReferences(IUltraCalcReference reference, int connect)
		{
			foreach (UltraCalcAncestorMap.AncestorMapEntry predecessorEntry in AncestorMap) 
			{
				// Disconnecting?
				if (connect == 0)
				{
					if ( reference.ContainsReference( predecessorEntry.Predecessor.Reference ) && predecessorEntry.Predecessor.IsConnected )
					{
						// Dirty any references to the disconnected reference.  We need to do this before disconnecting so we have the fully 
						// qualified absolute name of the soon to be disconnected references.
						//
						// Note we just dirty references to the resolved predecessor in order to minimally pick up only affected references to the 
						// disconnected row(s) collection.

						DirtyParents(reference.ResolveReference(predecessorEntry.Predecessor.Reference, ResolveReferenceType.RightHandSide), false);

						// Disconnect all references to this predecessor within each its ancestor's formulas.
						ConnectAncestorReferences( predecessorEntry.Predecessor, predecessorEntry.Ancestors, connect );
					} 
				}
					// Connecting?
				else if (connect == 1)
				{
					// Since we don't have the fully qualified absolute name of the disconnected predecessors, we need to attempt to 
					// connect any disconnected reference
					if ( !predecessorEntry.Predecessor.IsConnected )
					{
						// First, connect the reference to this predecessor within each its ancestor's formulas.
						ConnectAncestorReferences(predecessorEntry.Predecessor, predecessorEntry.Ancestors, connect);

						// Dirty any references to the predecessor
						DirtyParents(reference.ResolveReference(predecessorEntry.Predecessor.Reference, ResolveReferenceType.RightHandSide), false);
					}
				}
				else
				{
					// Reconnecting?
                    // MRS 12/2/2010 - TFS59821
                    // What happened here was that the sample is defining a formula at design time on 
                    // a control (it happens to be UltraCurrencyEditor, but it doesn't really matter) 
                    // which references a cell in the grid. Since the formula is defined at 
                    // design-time, it is getting added to the Calc network before the grid rows have 
                    // been created. 
                    // The grid cell reference therefore fails to get connected. When the grid's 
                    // DataSource gets set, the grid fires a Resynch, and we get here, but we once 
                    // again fail to connect the cell reference, because it was never connected in
                    // the first place. 
                    // So I am removing the check for IsConnected here. This will cause a slight 
                    // performance hit, but only in a case where there's a reference that has never
                    // been properly connected, which should be a very rare and unusual case.
                    //
					//if ( predecessorEntry.Predecessor.IsConnected && reference.ContainsReference( predecessorEntry.Predecessor.Reference ) )
                    if (reference.ContainsReference(predecessorEntry.Predecessor.Reference))
					{
						// Reconnect reference to this predecessor within each its ancestor's formulas.
						ConnectAncestorReferences( predecessorEntry.Predecessor, predecessorEntry.Ancestors, connect );


						// Dirty any direct references to the predecessor that we've just reconnected
						DirtyParents(reference.ResolveReference(predecessorEntry.Predecessor.Reference, ResolveReferenceType.RightHandSide), false);
					}

				}				
			}			

			// Now remove any calculated references contained by the reference from the recalc chain
			if (recalcChain.Count > 0)
			{
				foreach ( IUltraCalcReference formulaReference in FormulasSubsetOf( reference, recalcChain ) ) 
					recalcChain.Remove( formulaReference );
			}

			// Add any calculated columns contained by the connected or reconnected references to the recalc chain
			if ( (connect == 1 || connect == -1) && this.AncestorMap.Count > 0 )
			{
				foreach (UltraCalcAncestorMap.AncestorMapEntry predecessorEntry in AncestorMap) 
				{
					foreach (FormulaReference formulaReference in FormulasContainedBy(reference, predecessorEntry.Ancestors)) 
					{
						// unwrap the reference first
						IUltraCalcReference formulaRefResolved = formulaReference.UnderlyingReference;

						// AS 9/17/04
						// If the reference in the ancestor map had a formula but the formula
						// on the reference has since been changed, skip this reference.
						if (formulaRefResolved.Formula != formulaReference.Formula)
							continue;

						IUltraCalcReference convReference = reference.ResolveReference(formulaRefResolved, ResolveReferenceType.LeftHandSide); 

						// SSP 10/6/04
						// Got rid of RecalcListed, RecalcDirty and RecalcMarked flags.
						//
						//if (!convReference.RecalcDirty)
						if ( ! this.recalcChain.IsRecalcListed( convReference ) )
							DirtyParents(convReference, true);
					}
				}
			}

		}

		#endregion //ConnectReferences

		#region SortReferences



#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

		private void SortReferences( IUltraCalcReference reference )
		{
			foreach ( UltraCalcAncestorMap.AncestorMapEntry predecessorEntry in AncestorMap )
				// Connect or disconnect references contained by the given reference and recalc any ancestors of these references
				if ( predecessorEntry.Predecessor.IsConnected && reference.ContainsReference( predecessorEntry.Predecessor.Reference ) )
				{
					// Does reference contain a relative reference?
					if ( predecessorEntry.Predecessor.HasRelativeIndex )
					{
						DirtyParents( reference.ResolveReference( predecessorEntry.Predecessor.Reference, ResolveReferenceType.RightHandSide ), false );
					}
					// Does reference contain a absolute reference?
					if ( predecessorEntry.Predecessor.HasAbsoluteIndex )
					{
						ConnectAncestorReferences( predecessorEntry.Predecessor, predecessorEntry.Ancestors, -1 );
						DirtyParents( reference.ResolveReference( predecessorEntry.Predecessor.Reference, ResolveReferenceType.RightHandSide ), false );
					}
				}
		}

		#endregion //SortReferences 

		#region DirtyParents
		


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		// MD 3/16/09 - TFS14252
		// Added a return type of bool to indicate whether any parents were actually dirtied.
		//private void DirtyParents(IUltraCalcReference predecessor, bool inclusive)
		private bool DirtyParents( IUltraCalcReference predecessor, bool inclusive )
		{
			// MD 3/16/09 - TFS14252
			// The method now has a return value.
			bool returnValue = false;

			// unwrap the reference if necessary
			IUltraCalcReference reference = predecessor is FormulaReference ? ((FormulaReference)predecessor).UnderlyingReference : predecessor;

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

					// AS 10/11/04
					// The RecalcListed/RecalcDirty flags are no longer used.
					//DirtyChildren(predecessor.Formula);
					// SSP 7/13/11 - XamCalculationManager
					// 
					//this.OnDirtyValue( new UltraCalcReferenceEventArgs( reference ) ); 
					this.OnDirtyValue( reference ); 
				}
			}

			// MD 11/1/10 - TFS56976
			// If we can prevent dirtying parents, we can return here. We can't return at the beginning of the method though because when 
			// the inclusive parameter it passed in as True, we always want to add the reference to the recalc chain.
			if (this.preventDirtyParents)
				return false;

			// MD 1/12/12 - TFS99279
			// Refactored some of this code into the UltraCalcAncestorMap.Ancestors method so we can use these performance enhancements 
			// in other places.
			#region Refactored

			//#if EXCEL
			//            // MD 8/22/08 - Excel formula solving - Performance
			//            // Instead of always searching through each entry, always search through the non-cell predecessor 
			//            // entries and if the predecessor is a cell reference, just dirty that one cell reference.
			//            IUltraCalcReference resolvedPredecessor = UltraCalcEngine.GetResolvedReference( reference );
			//            CellCalcReference cellReference = resolvedPredecessor as CellCalcReference;

			//            if ( cellReference != null )
			//            {
			//                UltraCalcAncestorMap.AncestorMapEntry entry;
			//                if ( this.ancestorMap.CellPredecessorEntries.TryGetValue( cellReference, out entry ) )
			//                {
			//                    // MD 3/16/09 - TFS14252
			//                    // If the cell reference is a predecessor, the method should return true.
			//                    returnValue = true;

			//                    this.DirtyAncestors( predecessor, entry.Ancestors );
			//                }

			//                // MD 11/1/10 - TFS56976
			//                // Search through the new collection of region predecessors for regions containing the cell.
			//                // MD 4/12/11 - TFS67084
			//                // Moved away from using WorksheetCell objects.
			//                //List<RegionCalcReference> regionCalcReferences = this.ancestorMap.RegionPredecessorEntries.GetRegionsContainingCell(cellReference.Cell);
			//                List<RegionCalcReference> regionCalcReferences = this.ancestorMap.RegionPredecessorEntries.GetRegionsContainingCell(cellReference.Row, cellReference.ColumnIndex);

			//                if (regionCalcReferences != null)
			//                {
			//                    returnValue = true;

			//                    // Dirty the parents of each region reference that contains the cell.
			//                    foreach (RegionCalcReference regionCalcReference in regionCalcReferences)
			//                    {
			//                        if (this.ancestorMap.AncestorDictionary.TryGetValue(regionCalcReference, out entry) == false)
			//                        {
			//                            Utilities.DebugFail("Could not find the RegionCalcReference in the AncestorDictionary.");
			//                            continue;
			//                        }

			//                        this.DirtyAncestors(regionCalcReference, entry.Ancestors);
			//                    }
			//                }
			//            }

			//            // MD 11/1/10 - TFS56976
			//            // Now that region predecessors are stored in a different collection, we should verify the assumption that cell references are the only
			//            // things that are dirtied that could intersect with the region.
			//            Debug.Assert(
			//                cellReference != null || resolvedPredecessor is NamedCalcReference || resolvedPredecessor.Value.IsError,
			//                "Didn't expect somethig other than a cell, named reference or error to be dirtied: " + resolvedPredecessor.GetType().Name);


			//            // MD 11/1/10 - TFS56976
			//            // Wrapped this foreach loop in an if statement. There's no point to creating and disposing an enumerator when there are no items.
			//            if(this.ancestorMap.NonCellPredecessorEntries.Count > 0)
			//            {
			//                foreach ( UltraCalcAncestorMap.AncestorMapEntry entry in this.ancestorMap.NonCellPredecessorEntries )
			//                {
			//                    if ( entry.Predecessor.ContainsReference( reference ) )
			//                    {
			//                        // MD 3/16/09 - TFS14252
			//                        // If the cell reference is contained in a predecessor, the method should return true.
			//                        returnValue = true;

			//                        this.DirtyAncestors( predecessor, entry.Ancestors );
			//                    }
			//                }
			//            }
			//#else
			//            // Search for ancestors of the given predecessor 
			//            foreach (UltraCalcAncestorMap.AncestorMapEntry entry in ancestorMap)
			//            {
			//                // Does this entry "contain" the given predecessor?
			//                if (entry.Predecessor.ContainsReference(reference))
			//                {
			//                    // MD 3/16/09 - TFS14252
			//                    // If the reference is contained in a predecessor, the method should return true.
			//                    returnValue = true;

			//                    // MD 8/22/08 - Excel formula solving - Performance
			//                    // Moved to DirtyAncestors helper method
			//                    #region Moved

			//                    //// Add each ancestor to the dirty chain
			//                    //foreach (IUltraCalcReference ancestor in entry.Ancestors) 
			//                    //{
			//                    //    // Resolve ancestor reference against its predecessor
			//                    //    IUltraCalcReference convAncestor = predecessor.ResolveReference( ((FormulaReference)ancestor).UnderlyingReference, ResolveReferenceType.LeftHandSide );
			//                    //    Debug.Assert( null != convAncestor, "Null returned from ResolveReference !" );
			//                    //
			//                    //    // Not already on the dirty chain?
			//                    //    // SSP 10/6/04
			//                    //    // Got rid of RecalcListed, RecalcDirty and RecalcMarked flags.
			//                    //    //
			//                    //    //if (!(convAncestor is UltraCalcReferenceError) && !convAncestor.RecalcDirty) 
			//                    //    if ( null != convAncestor && !( convAncestor is UltraCalcReferenceError ) ) 
			//                    //    {
			//                    //        if (recalcChain.Add(convAncestor))
			//                    //            this.OnDirtyValue( new UltraCalcReferenceEventArgs(convAncestor) );
			//                    //    }
			//                    //} 

			//                    #endregion Moved
			//                    this.DirtyAncestors( predecessor, entry.Ancestors );
			//                }
			//            }
			//#endif

			#endregion  // Refactored
			UltraCalcReferenceCollection ancestors = this.AncestorMap.Ancestors(reference);
			returnValue = ancestors.Count != 0;
			this.DirtyAncestors(reference, ancestors);

			// MD 3/16/09 - TFS14252
			// The method now has a return value.
			return returnValue;
		}

		#endregion //DirtyParents

		

		// MD 8/22/08 - Excel formula solving - Performance
		// Moved from DirtyParents
		#region DirtyAncestors

		private void DirtyAncestors( IUltraCalcReference predecessor, UltraCalcReferenceCollection ancestors )
		{
			// Add each ancestor to the dirty chain
			foreach ( IUltraCalcReference ancestor in ancestors )
			{
				// Resolve ancestor reference against its predecessor
				IUltraCalcReference convAncestor = predecessor.ResolveReference(((FormulaReference)ancestor).UnderlyingReference, ResolveReferenceType.LeftHandSide);
				Debug.Assert( null != convAncestor, "Null returned from ResolveReference !" );

				// Not already on the dirty chain?
				// SSP 10/6/04
				// Got rid of RecalcListed, RecalcDirty and RecalcMarked flags.
				//
				//if (!(convAncestor is UltraCalcReferenceError) && !convAncestor.RecalcDirty) 
				if ( null != convAncestor && !( convAncestor is UltraCalcReferenceError ) )
				{
					if ( recalcChain.Add( convAncestor ) )
					{
						// SSP 7/13/11 - XamCalculationManager
						// 
						//this.OnDirtyValue( new UltraCalcReferenceEventArgs( convAncestor ) ); 
						this.OnDirtyValue( convAncestor ); 
					}
				}
			}
		} 

		#endregion DirtyAncestors

		#region ClearDirtyChain
		


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		private void ClearDirtyChain( )
		{
			// SSP 10/6/04
			// Commented out the original code and added the new one.
			//
			if ( this.recalcChain.HasDirtyItems )
			{
				for ( int i = this.recalcChain.DirtyChainStartIndex; i < this.recalcChain.Count; i++ )
					this.DirtyParents( this.recalcChain[i], false );

				// Clear the dirty chain start index.
				//
				this.recalcChain.ResetDirtyChainStartIndex( );

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

		#region TopologicalSort

		// SSP 9/27/04 - Circular Relative Index Support 
		// Rewrote the TopologicalSort method. The original method is commented out.
		//


#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)

		private void TopologicalSort( )
		{
			this.recalcChain.Sort( new RecalcChainSortComparer( ) );

			// Denote we've sorted the list.
			//
			this.doTopologicalSort = false;
		}

		// Here is the original TopologicalSort.
		//
		
#region Infragistics Source Cleanup (Region)




















































































































#endregion // Infragistics Source Cleanup (Region)

	
		#endregion // TopologicalSort

		#region Evaluate

		// SSP 9/27/04
		// Implemented being able to use relative indexes in a circular way like
		// in A=B(-1), B=A formulas and evaluating formulas across rows.
		// Rewrote the Evaluate method. Original code is commented out.
		//


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		private void Evaluate(long ticks) 
		{
			// MD 8/12/08 - Excel formula solving
			// If we are already evaluating a formula, don't do anything.
			if ( this.isEvaluating )
			{
				CalcManagerUtilities.DebugFail( "Evaluate should not be called when we are already evaluating." );
				return;
			}

			// MD 8/12/08 - Excel formula solving
			// Set the anti-recursion flag. Wrapped the code below in a try...finally, so the flag could be reset after the method is done.
			this.isEvaluating = true;
			try
			{

            // MRS 2/4/2008 - BR30242
            this.DebugOutputEvaluate();

			// MD 8/22/08 - Excel formula solving - Performance
			// This code is not needed in Excel
			if ( null != this.pendingCalcInfo )
			{
				// Get current ticks
				long recalcTicks = DateTime.Now.Ticks;
				bool changeDetected = false;
				bool finishedEvaluatingFormulaGroup = false;
				
				// Enumerate the cells within the column and apply formula
				while ( -1 == ticks || DateTime.Now.Ticks - recalcTicks < ticks )
				{
					if ( null == this.pendingCalcInfo || ! this.pendingCalcInfo.EvaluateNextRow( ) )
					{
						changeDetected = null == this.pendingCalcInfo || this.pendingCalcInfo.ChangeDetected;
						if ( ! changeDetected )
							finishedEvaluatingFormulaGroup = true;

						break;
					}
				}

				if ( changeDetected || finishedEvaluatingFormulaGroup )
				{
					// If we stopped because we detected some change, then readd the references back to
					// the recalc chain.
					//
					if ( changeDetected ) 
					{
						// Put back the formulas we had popped off from the recalc chain but we didn't
						// finish evaluating them because some changes were detected causing us to 
						// prematurely stop the evaluation.
						//
						if ( null != this.pendingCalcInfo )
							this.pendingCalcInfo.CancelRangeRecalc( );
					}

					if ( ! changeDetected )
					{
						// Raise an event column has been cleaned
						for ( int i = 0; i < this.pendingCalcInfo.referencesBeingEvaluated.Length; i++ )
						{
							if ( null != this.pendingCalcInfo.referencesBeingEvaluated[i] )
								// SSP 7/13/11 - XamCalculationManager
								// 
								//this.OnRecalcValue( new UltraCalcReferenceEventArgs(this.pendingCalcInfo.referencesBeingEvaluated[i]) );
								this.OnRecalcValue( this.pendingCalcInfo.referencesBeingEvaluated[i] );
						}
					}

					// Clear the pending recalc info structure.
					//
					this.pendingCalcInfo = null;
				}
			}
			else
			{
				// MD 7/25/08 - Excel formula solving
				int referenceIndex = 0;

				// MD 7/25/08 - Excel formula solving
				//IUltraCalcReference reference = this.recalcChain[ 0 ]; 
				IUltraCalcReference reference = this.recalcChain[ referenceIndex ]; 

				// MD 8/17/08 - Excel formula solving
				// Refactoring - Renamed UltraCalcFormula to UltraCalcFormulaBase
				//UltraCalcFormula referenceFormula = (UltraCalcFormula)reference.Formula;
				UltraCalcFormulaBase referenceFormula = (UltraCalcFormulaBase)reference.Formula;

				Debug.Assert( null != referenceFormula, string.Format( "Reference {0} without formula encountered on the recalc chain !", reference.AbsoluteName ) );
				if ( null == referenceFormula )
				{
					// SSP 1/5/04
					// Instead of simply returning remove the item otherwise we'll get into an
					// infinite loop. Also to aid implementors of IUltraCalcReference in detecting
					// the problem throw and catch an exception.
					//
					// MD 7/25/08 - Excel formula solving
					// The first item might not be recalculated anymore. Remove the item at the index where the 
					// recalculation is occurring.
					//this.recalcChain.RemoveAt( 0 );
					this.recalcChain.RemoveAt( referenceIndex );

					try
					{
						throw new UltraCalcException( "A reference without a formula encountered in the recalc chain." );
					}
					catch
					{
					}

					return;
				}

				// Is the reference to a cell?
				if (!reference.IsEnumerable) 
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
					UltraCalcValue value;

					int originalCount = this.recalcChain.Count;
					Debug.Assert( referenceIndex < this.recalcChain.Count );

					
						Debug.Assert( originalCount == this.recalcChain.Count );

						// If the formula is a circular formula and iterative calculations are disabled, it must get a circularity error.
						// Otherwise, evaluate the formula.
						if ( referenceFormula.IsCircularFormula && this.IterativeCalculationsEnabled == false )
						{
							{
								value = new UltraCalcValue( new UltraCalcErrorValue( UltraCalcErrorCode.Circularity ) );
							}
						}
						else
						{
							value = referenceFormula.Evaluate( reference );
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
					{
						// MD 4/2/12 - TFS99854
						// Make sure the recalcChain wasn't re-ordered.
						Debug.Assert(this.recalcChain[referenceIndex] == reference, "The recalc chain order has changed.");

						// MD 7/25/08 - Excel formula solving
						// Remove the item at the index where the recalculation was occurring and set the calculated value on the 
						// reference.
						this.recalcChain.RemoveAt( referenceIndex );

						// MD 8/20/08 - Excel formula solving
						// The value will now be set in the AfterFormulaEvaluationCompleted method
						//reference.Value = value;
						this.AfterFormulaEvaluationCompleted( reference, referenceFormula, value );

						// Raise an event that its value has been cleaned
						// SSP 7/13/11 - XamCalculationManager
						// 
						//this.OnRecalcValue( new UltraCalcReferenceEventArgs( reference ) );
						this.OnRecalcValue( reference );

						// Denote we've calculated the visible cell
						if ( enableMarkedColumns )
							reference.RecalcVisible = false; 
					}
				} 
				else
				{
					// Create a range calc info structure and return. Recalc method will call this method
					// back if the time hasn't expired and we'll evaluate the range calc.
					//
					this.pendingCalcInfo = RangeCalcInfo.CreateNew( this.recalcChain );
					Debug.Assert( null != pendingCalcInfo ); 
				}
			}
			// MD 8/12/08 - Excel formula solving
			// Reset the anti-recursion flag when the method is done.
			}
			finally
			{
				this.isEvaluating = false;
			}
		}

		
#region Infragistics Source Cleanup (Region)



























































































#endregion // Infragistics Source Cleanup (Region)


		#endregion // Evaluate

		#region CompareReferences

		internal static bool CompareReferences( IUltraCalcReference reference1, IUltraCalcReference reference2 )
		{
			// SSP 10/7/04
			// Use Equals method on the references instead of the absolute name comparison. Now we are
			// requiring the reference implementors to override GetHashCode and Equals methods and 
			// implement them properly.
			//

			FormulaReference fr1 = reference1 as FormulaReference;
			FormulaReference fr2 = reference2 as FormulaReference;
			if ( null != fr1 )
				reference1 = fr1.UnderlyingReference;
			if ( null != fr2 )
				reference2 = fr2.UnderlyingReference;

			// If the second reference is an UCReference swap reference1 and reference2 to ensure that
			// we call Equals on the UCReference. UCReference.Equals has logic to compare it's underlying 
			// reference with the passed in reference.
			//
			if ( reference2 is UCReference )
			{
				IUltraCalcReference tmp = reference1;
				reference1 = reference2;
				reference2 = tmp;
			}
            
			return reference1 == reference2 || null != reference1 && null != reference2 && reference1.Equals( reference2 );
			
#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)

		}

		#endregion // CompareReferences

		#region Circular Relative Index Support & Optimizations

		// SSP 9/27/04
		// Implemented being able to use relative indexes in a circular way like
		// in A=B(-1), B=A formulas. Also added various optimizations that improves
		// performance a great deal.
		//

		#region GetFormulas







		private ReferenceHolder[] GetFormulas( )
		{
			HashSet formulas = new HashSet( );

			foreach ( UltraCalcAncestorMap.AncestorMapEntry entry in this.AncestorMap )
			{
				foreach ( FormulaReference formulaReference in entry.Ancestors )
				{
					// MD 8/17/08 - Excel formula solving
					// Refactoring - Renamed UltraCalcFormula to UltraCalcFormulaBase
					//UltraCalcFormula formula = (UltraCalcFormula)formulaReference.Formula;
					UltraCalcFormulaBase formula = (UltraCalcFormulaBase)formulaReference.Formula;

					Debug.Assert( null != formula && null != formula.BaseReference 
						&& formula == formula.BaseReference.Formula );

					if ( null != formula && null != formula.BaseReference && ! formulas.Exists( formula ) )
						formulas.Add( formula );
				}
			}

			ReferenceHolder[] formulaReferences = new ReferenceHolder[ formulas.Count ];
			int index = 0;

			// MD 8/17/08 - Excel formula solving
			// Refactoring - Renamed UltraCalcFormula to UltraCalcFormulaBase
			//foreach ( UltraCalcFormula formula in formulas )
			foreach ( UltraCalcFormulaBase formula in formulas )
				formulaReferences[ index++ ] = new ReferenceHolder( formula.BaseReference, formula );

			return formulaReferences;
		}

		#endregion // GetFormulas

		#region AncestorMapVersion






		internal int AncestorMapVersion
		{
			get
			{
				return this.ancestorMapVersion;
			}
		}

		#endregion // AncestorMapVersion

		#region BumpAncestorMapVersion






		internal void BumpAncestorMapVersion( )
		{
			this.ancestorMapVersion++;
		}

		#endregion // BumpAncestorMapVersion

		#region MarkRelativeIndices

		// SSP 7/12/05
		// We need to remark the tokens of a formula as whenever a token gets connected. This is because
		// UCReference when disconnected always returns false from HasRelativeIndex and HasAbsoluteIndex
		// causing the base reference' RefParser tuples to be not 'marked'. However when the token gets 
		// connected we need to mark the tuples connected again.
		// 
		internal void MarkRelativeIndices( IUltraCalcFormula formula )
		{
			IUltraCalcReference baseReference = formula.BaseReference;

			foreach ( UCReference reference in formula.References )
			{
				// SSP 11/9/04 UWC113
				// Also mark the base reference if any of its tokens has a scope of Index.
				//
				//if( reference.HasRelativeIndex ) 
				if ( reference.HasRelativeIndex || reference.HasAbsoluteIndex )
					reference.MarkRelativeIndices( baseReference );
			}
		}

		#endregion // MarkRelativeIndices 

		#region VerifyAncestorMap



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		// MD 8/20/08 - Excel formula solving
		// Changed the return type to a bool
		//private void VerifyAncestorMap( )
		private bool VerifyAncestorMap()
		{
			if ( this.verifiedAncestorMapVersion == this.ancestorMapVersion )
			{
				// MD 8/20/08 - Excel formula solving
				// Changed the return type to a bool
				//return;
				return false;
			}

			this.verifiedAncestorMapVersion = ancestorMapVersion;

			// SSP 6/27/07 - BR24314 - Optimizations
			// 
			AncestorMapCache cache = new AncestorMapCache( this );

			ReferenceHolder[] formulas = this.GetFormulas( );
	
			// Reset various info on all the formulas.
			//
			foreach ( ReferenceHolder formulaReference in formulas )
			{
				// SSP 7/12/05
				// We need to remark the tokens of a formula as whenever a token gets connected. This is because
				// UCReference when disconnected always returns false from HasRelativeIndex and HasAbsoluteIndex
				// causing the base reference' RefParser tuples to be not 'marked'. However when the token gets 
				// connected we need to mark the tuples connected again.
				// 
				this.MarkRelativeIndices( formulaReference.Formula );	 		

				// Set the BaseReferenceHolder temporarily during the process of ancestor
				// map verification so we can use it throughout the logic.
				//
				formulaReference.Formula.BaseReferenceHolder = formulaReference;
				formulaReference.Formula.DependancySortNumber = -1;

				formulaReference.Formula.EvaluationGroupNumber = -1;
				formulaReference.Formula.EvaluationRowOffset = 0;
				formulaReference.Formula.RowIterationType = RowIterationType.Any;

				// SSP 12/16/04 BR01209
				// Store the original RecalcDeferred value of the formula.
				//
				formulaReference.origRecalcDeferred = formulaReference.Reference.RecalcDeferred;

				// Initialize the RecalcDeferred to true on all the formula references. 
				// AssignFormulaSortNumbers call below will set these to false on references
				// that can't be recalc deferred.
				//
				formulaReference.Reference.RecalcDeferred = this.EnableMarkedColumns;
			}

			ReferenceHolder circularFormula = null; 

			// MD 8/20/08 - Excel formula solving
			// Clear the collection of circular references. It will be regenerated when the circularities are detected.
			this.circularReferences.Clear();

			// Then detect circularities. This also assigns the RequiredRowIterationType on 
			// the formulas.
			// 
			foreach ( ReferenceHolder formulaReference in formulas )
			{
				// SSP 6/27/07 - BR24314 - Optimizations
				// Added cache parameter. Pass that along.
				// 
				//if ( this.DetectCircularity( formulaReference ) )
				if ( this.DetectCircularity( cache, formulaReference ) )
				{
					circularFormula = formulaReference;

					// MD 8/20/08 - Excel formula solving
					// If circularities are supported by the calc engine, continue detecting circularities to find them all.
					//break;
					if ( this.AllowCircularities == false )
						break;
				}
			}

			// Assign sibling numbers to the formulas. Also sort the array so all the siblings
			// appear together.
			//
			this.AssignFormulaSiblingNumbers( formulas ); 
			
			// MD 8/20/08 - Excel formula solving
			bool assignFormulaSortNumbers = true;

			// If the formula has a logical circularity then return without assigning sort numbers.
			// Nothing will be calculated until the circularity is resolved.
			//
			if ( null != circularFormula )
			{
				Debug.WriteLine( string.Format( "Circularity detected on {0} formula.", circularFormula.Formula.FormulaString ) );
				this.circularReference = circularFormula.Formula.BaseReference;

				// SSP 7/13/11 - XamCalculationManager
				// 
				//this.OnCircularity( new UltraCalcReferenceEventArgs( this.circularReference ) ); 
				this.OnCircularity( this.circularReference ); 

				// MD 8/20/08 - Excel formula solving
				// If circularities are not supported by the calc engine, the formula sort numbers should not be assigned below. This
				// is needed not because the call to AssignFormulaSortNumbers was moved out of the else block below.
				if ( this.AllowCircularities == false )
					assignFormulaSortNumbers = false;
			}
			else
			{
				// SSP 12/15/04
				// Reset the circularReference member var to null since we did not detect any circularity.
				//
				this.circularReference = null;

				// Assign sort numbers to the formulas.
				//
				// SSP 6/27/07 - BR24314 - Optimizations
				// Added cache parameter. Pass that along.
				// 
				//this.AssignFormulaSortNumbers( formulas );
				// MD 8/20/08 - Excel formula solving
				// This is being handled outside the if block now
				//this.AssignFormulaSortNumbers( cache, formulas );
			}

			// MD 8/20/08 - Excel formula solving
			// Moved from the else block above so the formula sort numbers could be assigned to circular references when circularities are allowed.
			if ( assignFormulaSortNumbers )
				this.AssignFormulaSortNumbers( cache, formulas );

			// Set the BaseReferenceHolder to null on all the formulas after the process of ancestor
			// map verification is finished.
			//
			foreach ( ReferenceHolder formulaReference in formulas )
			{
				// SSP 12/16/04 BR01209
				// We need to add formulas whose recalc deferred state goes from true to false
				// to the recalc chain so they get recalculated fully. The main reason for doing
				// this is that if a new formula gets added that refers to an existing recalc
				// deferred formula then we need to make sure that the whole of the existing
				// formula gets calculated before the added formula is calculated. For example,
				// if a summary is added that refers to an existing recalc deferred column formula
				// that has been cleared off the recalc chain, we need to readd it so all the 
				// cells of the column get calculated because only the ones that came into view 
				// would have gotton calculated.
				//
				// ------------------------------------------------------------------------------
				if (
						null != formulaReference.Reference
						&& formulaReference.origRecalcDeferred 
						&& !formulaReference.Reference.RecalcDeferred  
					)
					this.DirtyParents( formulaReference.Reference, true );
				// ------------------------------------------------------------------------------

				formulaReference.Formula.BaseReferenceHolder = null;
			}

			// MD 8/20/08 - Excel formula solving
			// Changed the return type to a bool
			return true;
		}

		#endregion // VerifyAncestorMap

		#region AssignFormulaSiblingNumbers



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private void AssignFormulaSiblingNumbers( ReferenceHolder[] formulas )
		{
			// Sort the formulas so all the non-enumerable formulas are before the enumerable formulas.
			//
			int i, j;
			for ( i = 0, j = 0; i < formulas.Length; i++ )
			{
				if ( formulas[i].Reference.IsEnumerable )
					TokenInfo.SwapItems( formulas, j++, i );
			}

			// Sort the formulas array so all the siblings are together in the array.
			//
			int siblingNumberCounter = 0;
			i = 0;
			while ( i < formulas.Length )
			{
				ReferenceHolder iiFormula = formulas[i];
				iiFormula.Formula.SiblingNumber = ++siblingNumberCounter;

				int startIndex = i;

				for ( j = 1 + i; j < formulas.Length; j++ )
				{
					ReferenceHolder jjFormula = formulas[j];
					if ( iiFormula.Formula.BaseReference.IsSiblingReference( jjFormula.Formula.BaseReference ) )
					{
						jjFormula.Formula.SiblingNumber = siblingNumberCounter;
						TokenInfo.SwapItems( formulas, ++i, j );
					}
				}

				if ( startIndex != i )
                    iwu.SortMerge(formulas, null, 
						new TokenInfo.SiblingFormulaSortComparer( ), startIndex, i );

				i++;
			}
		}

		#endregion // AssignFormulaSiblingNumbers

		#region DetectCircularity



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		// SSP 6/27/07 - BR24314 - Optimizations
		// Added cache parameter.
		// 
		//private bool DetectCircularity( ReferenceHolder formulaReference )
		private bool DetectCircularity( AncestorMapCache cache, ReferenceHolder formulaReference )
		{
			// SSP 6/27/07 - BR24314 - Optimizations
			// Added cache parameter.
			// 
			//HashSet selfDependantTokens = TokenInfo.GetDependants( formulaReference, formulaReference );
			HashSet selfDependantTokens = TokenInfo.GetDependants( cache, formulaReference, formulaReference );

			// If the formula refers to itself then set the refersBackToItself flag on
			// the formula reference holder to true so the AssignSiblingNumbers method 
			// can sort the siblings so such formulas are grouped after all the formulas
			// that do not refer to themselves. This is to increase the probability of
			// assigning smaller sort numbers to the recalc deferrable formulas so that
			// they can be calculated before non-recalc deferrable formulas among siblings.
			//
			formulaReference.refersBackToItself = ! selfDependantTokens.IsEmpty;

			// SelfDependantTokens contains the the base reference tokens that the formula
			// depends on. This will let us find out if the formula depends on itself. For
			// example, A=A is a circularity and D(A(0)->A) (meaning A dependants of A(0)) will
			// return {A(0)} as a set of tokens. This means that A(0) depends on A(0) and
			// that's a circularity. In the foreach loop below, we see if one of the tokens in
			// the returned set has the relative index of 0 and we consider that a circularity.
			// The same goes for formulas of type A=A(-1)+A(+5) where D(A(0)->A) will be
			// {A(-1),A(+5)} and we consider this as a circularity as well since the relative
			// indexes of the two tokens in the set have different signs. A=A(-1) is not
			// circular however. D(A(0)->A) in this case will return {A(-1)} which will pass
			// the tests below. A=sum(A(*)) is a circularity and D(A(0)->A) will return {A(*)}
			// and it will fail because the Scope will not be RelativeIndex and the condition
			// in the foreach loop below will fail.
			//
			bool circularityDetected = false;
			TokenInfo lastToken = null;

			foreach ( TokenInfo t in selfDependantTokens )
			{
				int scopeIndex = t.ScopeIndex;

				// The formula is circular if a self dependant token doesn't have the scope of
				// RelativeIndex (like in A=sum(A(*))) or if the scopeIndex is 0 (like in A=A) or
				// if there are two tokens where one's relative index is negative and other's is
				// positive (like in A=A(-1)+A(+2)).
				//
				if ( TokenInfo.ScopeType.RelativeIndex != t.Scope || 0 == scopeIndex
					// SSP 9/4/08 BR31006
					// Although this change doesn't have anything to do with this bug, I noticed
					// it while debugging it. To check if two values have different signs, use
					// multiplication instead of division because the operands are integer types
					// where -1 / 2 will result in 0.
					// 
					//|| null != lastToken && lastToken.ScopeIndex / scopeIndex < 0 
					|| null != lastToken && lastToken.ScopeIndex * scopeIndex < 0 
					)
				{
					circularityDetected = true;
					break;
				}

				lastToken = t;
			} 

			if ( !circularityDetected )
			{
				// If the formula is as such (like in A=A(-1)) that the cells depend on the
				// previous cells for their values then the previous cells must be calculated
				// before the later cells. For such formulas the cells have to be calculated from
				// top to bottom. For formulas where the cells require later cells to be calculated
				// first (like in A=A(+1)) then the cells have to be calculated from bottom to top.
				// If the formula doesn't refer to itself then then the cells can be calculated 
				// in any order.
				//
				if ( null == lastToken )
					formulaReference.Formula.RowIterationType = RowIterationType.Any;
				else if ( lastToken.ScopeIndex < 0 )
					formulaReference.Formula.RowIterationType = RowIterationType.Forward;
				else if ( lastToken.ScopeIndex > 0 )
					formulaReference.Formula.RowIterationType = RowIterationType.Backward;
				else
					// Can't get here because if the ScopeIndex on the token was 0 then
					// circularityDetected would have been set.
					//
					Debug.Assert( false );
			} 

			// MD 8/20/08 - Excel formula solving
			// Let the fomrula know when it is a circular formula.
			formulaReference.Formula.IsCircularFormula = circularityDetected;

			// If the formula is a circularity, add it to the dictionary on circular references.
			if ( circularityDetected )
				this.circularReferences[ formulaReference.Reference ] = null;

			return circularityDetected;
		}

		#endregion // DetectCircularity

		#region AssignFormulaSortNumbers



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		// SSP 6/27/07 - BR24314 - Optimizations
		// Added cache parameter.
		// 
		//private bool AssignFormulaSortNumbers( ReferenceHolder[] formulas )
		private bool AssignFormulaSortNumbers( AncestorMapCache cache, ReferenceHolder[] formulas )
		{
			int maxAssignedEvaluationGroupNumber = -1;
			int maxAssignedDependancySortNumber = -1;

			// Assign formula sort numbers to all the formulas. 
			//
			for ( int i = 0; i < formulas.Length; i++ )
			{
				if ( ! this.AssignFormulaSortNumber(
					// SSP 6/27/07 - BR24314 - Optimizations
					// Added cache parameter. Pass that along.
					// 
					cache,
					formulas, formulas[i], 
					ref maxAssignedDependancySortNumber, ref maxAssignedEvaluationGroupNumber ) )
					return false;
			}

			// Group formulas together in such a way that they can be calculated row-by-row.
			// Following assigns EvaluationGroupNumber and EvaluationRowOffset on all the
			// formulas. Evaluate logic will make use of these settings to determine which
			// formulas to group together when calculating them row-by-row.
			//
			int c = 0;
			for ( int i = 1; i < formulas.Length; i++ )
			{
				if ( formulas[c].Formula.SiblingNumber != formulas[i].Formula.SiblingNumber )
				{
					this.AssignFormulaGroupNumbersToSiblings(
						// SSP 6/27/07 - BR24314 - Optimizations
						// Added cache parameter. Pass that along.
						// 
						cache,
						formulas, c, i - 1, 
						ref maxAssignedDependancySortNumber, ref maxAssignedEvaluationGroupNumber );
					c = i;
				}
			}

			this.AssignFormulaGroupNumbersToSiblings(
				// SSP 6/27/07 - BR24314 - Optimizations
				// Added cache parameter. Pass that along.
				// 
				cache,
				formulas, c, formulas.Length - 1, 
				ref maxAssignedDependancySortNumber, ref maxAssignedEvaluationGroupNumber );



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


			return true;
		}

		#endregion // AssignFormulaSortNumbers

		#region AssignFormulaGroupNumbersToSiblings
		


#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

		private void AssignFormulaGroupNumbersToSiblings(
			// SSP 6/27/07 - BR24314 - Optimizations
			// Added cache parameter.
			// 
			AncestorMapCache cache,
			ReferenceHolder[] formulas, int startIndex, int endIndex,
			ref int maxAssignedDependancySortNumber, ref int maxAssignedEvaluationGroupNumber)
		{
			// Sort the formulas by DependancySortNumber.
			//
            iwu.SortMerge(formulas, null, 
				new TokenInfo.ReferenceHolderSortNumberComparer( ), startIndex, endIndex );

			int i;

			// If any sibling is not recalc deferrable then make all the siblings not recalc deferrable. 
			// This is optional in the sense that this is what was decided the behavior should be but not
			// necessary for the proper workings. If this behavior is deemed to be undesirable in the 
			// future, simply comment the following block out.
			//
			// ----------------------------------------------------------------------------------------
			bool allSiblingsRecalcDeferrable = true;
			for ( i = startIndex; allSiblingsRecalcDeferrable && i <= endIndex; i++ )
				allSiblingsRecalcDeferrable = formulas[i].Reference.RecalcDeferred;

			if ( ! allSiblingsRecalcDeferrable )
			{
				for ( i = startIndex; i <= endIndex; i++ )
					formulas[i].Reference.RecalcDeferred = false;
			}
			// ----------------------------------------------------------------------------------------

			// SSP 6/26/07 - BR24314 - Optimizations
			// 
			// --------------------------------------------------------------------------------------------
			// SSP 9/5/07 BR25911
			//
			i = startIndex;
			while ( i <= endIndex )
			{
				ReferenceHolder iiFormula = formulas[i];

				// Skip all the formulas that are involved in a circularity with iiFormula. You can tell 
				// that a series of consecutive formulas are involved in circularity if their 
				// EvaluationGroupNumber values are the same and non-negative. AssignFormulaSortNumber 
				// method assigns evaluation group numbers only to the formulas involved in circularity.
				// 
				if ( iiFormula.Formula.EvaluationGroupNumber >= 0 )
				{
					i++;
					continue;
				}

				// Check to see if formulas[i] can be calculated with the preceding formulas across
				// the rows. Formula like A=B, or A=B(-1) and B=C where A and B are sibling columns 
				// can be calculated row-by-row. However formula of A=sum(B(*)) and B=C can not be
				// calculated row-by-row because all the cells of B need to be calculated before A
				// can be calculated. Note: LastUnProcessedIndex - 1 indicates the end of last 
				// evaluation group. So the current group starts from lastUnProcessedIndex.
				//
				for ( int j = 1 + i; j <= 1 + endIndex; j++ )
				{
					ReferenceHolder jjFormula = j <= endIndex ? formulas[j] : null;

					bool groupEnded = null == jjFormula
						|| jjFormula.Formula.RowIterationTypeResolved != iiFormula.Formula.RowIterationTypeResolved
						|| jjFormula.Reference.RecalcDeferred != iiFormula.Reference.RecalcDeferred 
						|| jjFormula.Formula.EvaluationGroupNumber >= 0;

					if ( ! groupEnded )
					{
						HashSet allDependants = TokenInfo.GetDependants( cache, jjFormula, null );
						HashSet filteredDependants = null != allDependants 
							? TokenInfo.FilterDependants( allDependants, SubListEnumerable.SubElements( formulas, i, j - 1 ) )
							: null;

						if ( null != filteredDependants )
							groupEnded = TokenInfo.HasScopeOther( filteredDependants );
					}
					
					if ( groupEnded )
					{
						this.AssignGroupAndRowOffsets( cache, formulas, i, j - 1,
							ref maxAssignedDependancySortNumber, ref maxAssignedEvaluationGroupNumber );
						i = j;
						break;
					}
				}
			}
			
#region Infragistics Source Cleanup (Region)












































#endregion // Infragistics Source Cleanup (Region)

			// --------------------------------------------------------------------------------------------
		}

		#endregion // AssignFormulaGroupNumbersToSiblings

		#region AssignGroupAndRowOffsets
		


#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

		private void AssignGroupAndRowOffsets(
			// SSP 6/27/07 - BR24314 - Optimizations
			// Added cache parameter.
			// 
			AncestorMapCache cache,
			ReferenceHolder[] formulas, 
			int startIndex, int endIndex, 
			ref int maxAssignedDependancySortNumber, 
			ref int maxAssignedEvaluationGroupNumber )
		{
			// SSP 6/27/07 - BR24314 - Optimizations
			// Added cache parameter.
			// 
			//TokenInfo[] calculationMatrix = this.GetCalculationMatrix( formulas, startIndex, endIndex );
			TokenInfo[] calculationMatrix = this.GetCalculationMatrix( cache, formulas, startIndex, endIndex );

			// Assign sort numbers to the formulas. Also assign the same evaluation group number
			// because these formulas need to be calculated accross rows.
			//
			maxAssignedEvaluationGroupNumber++;
			for ( int i = 0; i < calculationMatrix.Length; i++ )
			{
				TokenInfo t = calculationMatrix[ i ];

				// MD 8/17/08 - Excel formula solving
				// Refactoring - Renamed UltraCalcFormula to UltraCalcFormulaBase
				//UltraCalcFormula formula = t.Formula;
				UltraCalcFormulaBase formula = t.Formula;

				// If dependancy sort order is not assigned, assign one.
				//
				if ( formula.DependancySortNumber < 0 )
					formula.DependancySortNumber = ++maxAssignedDependancySortNumber;

				// Assign the same evaluation group number to all the formulas in the array.
				//
				formula.EvaluationGroupNumber = maxAssignedEvaluationGroupNumber;

				// If the row iteration type is backward, meaning the cells have to be calculated from
				// bottom to top, then flip the row offsets. For example, in A=A(+1) formula, ScopeIndex
				// would be +1. However we want it to be -1 because the Evaluate logic simply loops 
				// cells backward.
				//
				formula.EvaluationRowOffset = RowIterationType.Backward == formula.RowIterationTypeResolved
					? -t.ScopeIndex : t.ScopeIndex;
			}
		}

		#endregion // AssignGroupAndRowOffsets
	
		#region AssignFormulaSortNumber



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		private bool AssignFormulaSortNumber(
			// SSP 6/27/07 - BR24314 - Optimizations
			// Added cache parameter.
			// 
			AncestorMapCache cache,
			ReferenceHolder[] allFormulas, 
			ReferenceHolder formulaReference,
			ref int maxAssignedDependancySortNumber,
			ref int maxAssignedEvaluationGroupNumber )
		{
			// MD 8/20/08 - Excel formula solving
			// Moved all code to the new overload
			return this.AssignFormulaSortNumber( 
				cache, 
				allFormulas, 
				formulaReference, 
				ref maxAssignedDependancySortNumber, 
				ref maxAssignedEvaluationGroupNumber, 
				new Dictionary<UltraCalcFormulaBase, object>() );
		}

		// MD 8/20/08 - Excel formula solving
		// Created a new overload with an extra parameter that prevents recursion when assigning sort numbers to circularly referenced formulas.
		private bool AssignFormulaSortNumber(
			// SSP 6/27/07 - BR24314 - Optimizations
			// Added cache parameter.
			// 
			AncestorMapCache cache,
			ReferenceHolder[] allFormulas,
			ReferenceHolder formulaReference,
			ref int maxAssignedDependancySortNumber,
			ref int maxAssignedEvaluationGroupNumber,
			Dictionary<UltraCalcFormulaBase, object> formulasBeingAssignedSortNumbers )
		{
			// If the formula is already assigned a dependancy sort number than return
			// without doing anything.
			//
			if ( formulaReference.Formula.DependancySortNumber >= 0 )
				return true;

			// MD 8/20/08 - Excel formula solving
			// We can now also get in here in circularities are allowed.
			//if ( ! formulaReference.refersBackToItself )
			if ( !formulaReference.refersBackToItself || this.AllowCircularities )
			{
				// Here the formula is not circular in any way, logically or otherwise. All we have to
				// ensure is that child tokens are assigned sort numbers and then get the max sort number
				// and assign one plus the max to this formula. This will effectively insure that all the
				// predecessor references have smaller numbers than their ancestor references.
				//
				// MD 8/17/08 - Excel formula solving
				// Refactoring - Renamed UltraCalcFormula to UltraCalcFormulaBase
				//UltraCalcFormula formula = formulaReference.Formula;			
				UltraCalcFormulaBase formula = formulaReference.Formula;

				// MD 8/20/08 - Excel formula solving
				// Prevent recursion by checking the dictionary of formulas which have sort numbers applied.
				if ( formulasBeingAssignedSortNumbers.ContainsKey( formula ) )
					return true;

				// If the formula was not in the dictionary, add it now.
				formulasBeingAssignedSortNumbers.Add( formula, null );

				// MD 7/29/08 - Excel formula solving
				// We shouldn't be using the formula tokens to find references. Not all references in the tokens are static references
				// so iterate all references actually used by the colletion.
				//foreach ( UltraCalcFormulaToken formulaToken in formula.Tokens )
				//{
				//    UltraCalcFormula childFormula;
				//    IUltraCalcReference childReference = TokenInfo.GetFormulaReferenceFromToken( formulaToken, out childFormula );
				foreach ( IUltraCalcReference childReference in formula.AllReferences )
				{
					// The childReference might be a UCReference. This was being unwrapped by the GetFormulaReferenceFromToken
					// which was claled before. It must now be unwrapped manually. All references to childReference in the rest 
					// of the loop have been replaced by references to childReferenceResolved.
					// MD 8/26/08 - BR35804
					// We only want to unwrap UCReferences.
					//IUltraCalcReference childReferenceResolved = UltraCalcEngine.GetResolvedReference( childReference );
					IUltraCalcReference childReferenceResolved = UltraCalcEngine.GetResolvedReference( childReference, true );

					// MD 8/17/08 - Excel formula solving
					// Refactoring - Renamed UltraCalcFormula to UltraCalcFormulaBase
					//UltraCalcFormula childFormula = childReferenceResolved.Formula as UltraCalcFormula;
					UltraCalcFormulaBase childFormula = childReferenceResolved.Formula as UltraCalcFormulaBase;

					// SSP 10/7/11
					// If the BaseReferenceHolder is null then that means the formula was not added to the ancestor
					// map. This can be the case when the formula has a syntax error. In which case we don't add
					// the formula to the ancestor map. We should skip such formulas.
					// 
					//if ( null != childFormula )
					if ( null != childFormula && null != childFormula.BaseReferenceHolder )
					{
						if ( childFormula.DependancySortNumber < 0 )
							this.AssignFormulaSortNumber(
								// SSP 6/27/07 - BR24314 - Optimizations
								// Added cache parameter. Pass that along.
								// 
								cache,
								allFormulas, 
								childFormula.BaseReferenceHolder,
								//new ReferenceHolder( childFormula.BaseReference, childFormula ),
								ref maxAssignedDependancySortNumber,
								// MD 8/20/08 - Excel formula solving
								// Added an extra parameter to this method to prevent recursion.
								//ref maxAssignedEvaluationGroupNumber );
								ref maxAssignedEvaluationGroupNumber,
								formulasBeingAssignedSortNumbers );

						// Assign recalc deferred flags on the references. Recalc deferred means the cells 
						// will be calculated as they come into view via the Visibility event.
						//
						if ( childReferenceResolved.RecalcDeferred )
						{
							if ( !formulaReference.Reference.RecalcDeferred
								|| !formulaReference.Reference.IsSiblingReference( childReferenceResolved ) )
							{
								// If an ancestor can't be recalc deferred then all its predecessors (immediate 
								// as well as non-immediate) can't be recalc deferred as well. Also if the 
								// predecessor is not a sibling of all its ancestors then it can't be recalc
								// deferred as well.
								//
								TokenInfo.SetRecalcDeferredToFalse( childFormula.BaseReferenceHolder );
							}
							else
							{
								// If the predecessor is being referred to using any scope except Any (
								// TokenInfo.GetScopeInfo translates Any into scope type of RelativeIndex
								// and scope index of 0) then the predecessor can't be recalc deferred.
								// 
								TokenInfo.ScopeType scope;
								int scopeIndex;
								TokenInfo.GetScopeInfo( childReferenceResolved, out scope, out scopeIndex );
								if ( TokenInfo.ScopeType.RelativeIndex != scope || 0 != scopeIndex )
									TokenInfo.SetRecalcDeferredToFalse( childFormula.BaseReferenceHolder );
							}
						} 

						// Sort number on the child formula should have been assigned at this point.
						//
						// MD 8/20/08 - Excel formula solving
						// This assert is not valid if the formula is a circular reference.
						//Debug.Assert( childFormula.DependancySortNumber >= 0 );
						Debug.Assert( childFormula.DependancySortNumber >= 0 || formulaReference.Formula.IsCircularFormula );
					}
				}

				Debug.Assert( formula.DependancySortNumber < 0 );
				if ( formula.DependancySortNumber < 0 )
					formula.DependancySortNumber = ++maxAssignedDependancySortNumber;
			}
			else
			{
				// The formula has a circularity. It can't be a logical circularity (a real circularity)
				// because circularities are detected before the assignment of formula sort numbers.
				// Create a list of formulas that are "involved" in this circularity. That is all the 
				// the formulas that refer back to themselves. FormulasInvolvedList variable will
				// contain ReferenceHolder instances.
				//
				ArrayList formulasInvolvedList = new ArrayList( );

				// Get all the dependant tokens from which we can create a list of references with
				// formulas that are "involved" in circularity. In A=B, B=C, C=B(-1) formulas A is
				// not involved in a circularity because D(A->A)={} where as B and C are involved in
				// circularity because D(B(0)->B)={B(-1)} and D(C(0)->C)={C(-1)}. In other words we
				// only are interested in B and C references in this case and not A. Note the
				// current formula being procssed can not be A because in the case of A we would 
				// not have gotten here since selfDependantTokens would have been empty above in the
				// begenning of the method.
				//
				// SSP 6/27/07 - BR24314 - Optimizations
				// Added cache parameter.
				// 
				//foreach ( ReferenceHolder rh in TokenInfo.GetReferenceHolders( TokenInfo.GetDependants( formulaReference, null ) ) )
				foreach ( ReferenceHolder rh in TokenInfo.GetReferenceHolders( TokenInfo.GetDependants( cache, formulaReference, null ) ) )
				{
					// We are only concerned with dependant references that have formulas. 
					// Dependant references with no formulas should never get on the recalc chain.
					//
					Debug.Assert( null != rh.Formula );
					if ( null != rh.Formula )
					{
						// Formulas with relative indexes and their predecessors shouldn't be recalc deferred.
						// Recalc deferred means the cells will be calculated as they come into view via
						// the Visibility event.
						//
						rh.Reference.RecalcDeferred = false;

						// SSP 6/27/07 - BR24314 - Optimizations
						// Added cache parameter.
						// 
						//HashSet refDeps = TokenInfo.GetDependants( rh, formulaReference );
						HashSet refDeps = TokenInfo.GetDependants( cache, rh, formulaReference );
						if ( ! refDeps.IsEmpty )
						{
							formulasInvolvedList.Add( rh );
						}
						else
						{
							// If the formula is not involved in the circularity (it simply happens to be a 
							// descendant of a formula involved in the circularity, then assign dependancy 
							// sort number to the formula. We have to make sure that the ancestors have 
							// greater sort numbers then their predecessors.
							//
							this.AssignFormulaSortNumber(
								// SSP 6/27/07 - BR24314 - Optimizations
								// Added cache parameter. Pass that along.
								// 
								cache,
								allFormulas, rh,
								// MD 8/20/08 - Excel formula solving
								// Added an extra parameter to this method to prevent recursion.
								//ref maxAssignedDependancySortNumber, ref maxAssignedEvaluationGroupNumber );
								ref maxAssignedDependancySortNumber, ref maxAssignedEvaluationGroupNumber,
								formulasBeingAssignedSortNumbers );

							// Sort number on the child formula should have been assigned at this point.
							//
							Debug.Assert( rh.Formula.DependancySortNumber >= 0 );
						}
					}
				}
				
				// At this point every formula in formulasInvolvedSet is circular. For example 
				// in A=A(-1)+B and B=C formulas, formulasInvolvedSet will contain A but not B.
				// 
				ReferenceHolder[] formulasInvolved = (ReferenceHolder[])formulasInvolvedList.ToArray( typeof( ReferenceHolder ) );
				formulasInvolvedList.Clear( );
				formulasInvolvedList = null;

				// Assign evaluation group and row offset numbers.
				//
				this.AssignGroupAndRowOffsets(
					// SSP 6/27/07 - BR24314 - Optimizations
					// Added cache parameter. Pass that along.
					// 
					cache,
					formulasInvolved, 0, formulasInvolved.Length - 1, 
					ref maxAssignedDependancySortNumber, ref maxAssignedEvaluationGroupNumber );
			}

			return true;
		}

		#endregion // AssignFormulaSortNumber

		#region GetCalculationMatrix

		// SSP 6/27/07 - BR24314 - Optimizations
		// Added cache parameter.
		// 
		//private TokenInfo[] GetCalculationMatrix( ReferenceHolder[] formulas, int startIndex, int endIndex )
		private TokenInfo[] GetCalculationMatrix( AncestorMapCache cache, ReferenceHolder[] formulas, int startIndex, int endIndex )
		{
			int formulaCount = 1 + endIndex - startIndex;
			if ( formulaCount <= 0 )
			{
				Debug.Assert( 0 == formulaCount );
				return new TokenInfo[] { };
			}

			RowIterationType rowIterationType = RowIterationType.Any;
			for ( int i = startIndex; i <= endIndex; i++ )
			{
				// MD 8/17/08 - Excel formula solving
				// Refactoring - Renamed UltraCalcFormula to UltraCalcFormulaBase
				//UltraCalcFormula formula = formulas[i].Formula;
				UltraCalcFormulaBase formula = formulas[ i ].Formula;

				// All the formulas must be siblings.
				//
				Debug.Assert( formulas[ startIndex ].Formula.SiblingNumber == formula.SiblingNumber ); 

				// In the passed in array, one formula can not have the row iteration type of Forward
				// and the other Backward.
				//
				Debug.Assert( RowIterationType.Any == rowIterationType
					|| RowIterationType.Any == formula.RowIterationType 
					|| rowIterationType == formula.RowIterationType );

				if ( RowIterationType.Any != formula.RowIterationType )
					rowIterationType = formula.RowIterationType;
			}

			// If all the formulas in the group require neither Forward nor Backward row iteration
			// then use the row iteration of Forward.
			//
			if ( RowIterationType.Any == rowIterationType )
				rowIterationType = RowIterationType.Forward;

			// Following code figures out which cells to calculate and in which order.
			// The for loop figures out the cells that 0th cells of all the columns depend
			// on. For example in A=A(-1) formula, A(0) cell depends on A(-1) cell. So the
			// following for loop will setup calculationMatrix array to contain A(-1). If
			// there are multiple A tokens that need to be calculated like in A=A(-1)+A(-2)
			// then the token with the largest relative index, A(-1) in this case, is
			// put in the calculationMatrix. CalculationMatrix only has one slot for each
			// formula so there can be only one token for each formula. After all such tokens
			// are calculated, we want to replace tokens with negative indexes with 0 index.
			// So the case of A=A(-1) formula will result calculationMatrix containing A(0).
			// NOTE: Above only applies to the row iteration of Forward. Row iteration
			// of Backward is similar except that we are trying to make sure that all the 
			// indexes in tokens are non-negative. We use rowOffsetDelta to later offset all
			// the indexes so that they are all non-positive in the case of Forward row 
			// iteration and non-negative in the case of Backward row iteration.
			//
			TokenInfo[] calculationMatrix = new TokenInfo[ formulaCount ];
			int rowOffsetDelta = 0;

			// SSP 6/27/07 - BR24314 - Optimizations
			// 
			HashSet[] dependantsArray = new HashSet[formulaCount];
			for ( int i = 0; i < formulaCount; i++ )
				dependantsArray[i] = TokenInfo.GetDependants( cache, formulas[ startIndex + i ], null );

			for ( int i = 0; i < formulaCount; i++ )
			{
				calculationMatrix[ i ] = TokenInfo.Create0Token( formulas[ startIndex + i ] );

				for ( int j = 0; j < formulaCount; j++ )
				{
					if ( i != j )
					{
						// SSP 6/27/07 - BR24314 - Optimizations
						// 
						//HashSet dependants = TokenInfo.GetDependants( formulas[ startIndex + j ], formulas[ startIndex + i ] );
						HashSet dependants = TokenInfo.FilterDependants( dependantsArray[ j ], formulas[ startIndex + i ] );

						if ( dependants.Count > 0 )
						{
							if ( null != calculationMatrix[ i ] )
								dependants.Add( calculationMatrix[ i ] );

							calculationMatrix[ i ] = 
								RowIterationType.Forward == rowIterationType 
								? TokenInfo.GetTokenWithMaxRelativeIndex( dependants )
								: TokenInfo.GetTokenWithMinRelativeIndex( dependants );
						}
					}
				}

				// Keep track of min or the max row offset depending on the row iteration type
				// so later we can use it to normalize the tokens. See the comment where it's 
				// being used further below.
				//
				if ( 0 != i )
				{
					if ( RowIterationType.Forward == rowIterationType )
						// If the row iteration type is Forward, then keep track of the max scope index.
						// Later we will make sure that row offsets on the formulas are no greater than 0 
						// because we want to make sure that all the cells of all the columns are calculated.
						// If for example a formula's RowEvaluationOffset was +1 then it's 0th cell wouldn't
						// get calculated unless it was made 0. In Evaluate logic we start iterating rows
						// from 0 and calculate formula cells by adding the RowEvaluationOffset to the current
						// row index. 
						//
						rowOffsetDelta = Math.Max( rowOffsetDelta, calculationMatrix[ i ].ScopeIndex );
					else
						// The same applies to Backward row iteration except in reverse. Here we want
						// to make sure that the row offsets on all the formulas are no less than 0.
						// In Evaluate logic we start iterating rows from the bottom so if a formula's
						// RowEvaluationOffset was -1 then its last cell would never get calculated.
						//
						rowOffsetDelta = Math.Min( rowOffsetDelta, calculationMatrix[ i ].ScopeIndex );
				}
				else
				{
					rowOffsetDelta = calculationMatrix[ i ].ScopeIndex;
				}
			}

			// Transform the token indexes (which we will use as row offsets in evaluate logic)
			// so that they are all non-positive in the case of Forward row iteration type and
			// non-negative in the case of the Backward row iteration type.
			// 
			for ( int i = 0; i < calculationMatrix.Length; i++ )
			{
				TokenInfo t = calculationMatrix[ i ];
				Debug.Assert( TokenInfo.ScopeType.RelativeIndex == t.Scope );
				calculationMatrix[ i ] = new TokenInfo( t.ReferenceHolder, t.Scope, t.ScopeIndex - rowOffsetDelta );
			}

			// Sort the tokens in the order in which they need to be calculated in Evaluate.
			//
			// SSP 6/27/07 - BR24314 - Optimizations
			// Added cache parameter.
			// 
			//TokenInfo.TokenSortComparer tokenSortComparer = new TokenInfo.TokenSortComparer( );
			TokenInfo.TokenSortComparer tokenSortComparer = new TokenInfo.TokenSortComparer( cache );

            iwu.SortMerge(calculationMatrix, tokenSortComparer);
			Debug.Assert( ! tokenSortComparer.circularityDetected );

			return calculationMatrix;
		}

		#endregion // GetCalculationMatrix

		#endregion  // Circular Relative Index Support & Optimizations

		#region SuspendNotificationProcessing



#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

		internal void SuspendNotificationProcessing()
		{
			this.suspendNotificationProcessingCounter++;
		}

		#endregion // SuspendNotificationProcessing 

		#region ResumeNotificationProcessing



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal void ResumeNotificationProcessing( bool dirtyAllFormulas )
		{
			if ( this.IsNotificationProcessingSuspended )
				this.suspendNotificationProcessingCounter--;

			if ( dirtyAllFormulas )
			{
				// SSP 6/27/07 BR23385
				// Since we ignore all the topological change events during suspension, we need to
				// reconnect all the tokens of all the formulas. For example, if a column gets 
				// added or deleted during suspension we would have simply ignored that change.
				// However when we resume, we need to make sure all the formulas reconnect their
				// references in order to resolve any added or deleted references.
				// 
				// ------------------------------------------------------------------------------------
				foreach ( UltraCalcAncestorMap.AncestorMapEntry predecessorEntry in this.AncestorMap )
					this.ConnectAncestorReferences( predecessorEntry.Predecessor, predecessorEntry.Ancestors, -1 );
				// ------------------------------------------------------------------------------------

				this.DirtyAllFormulas();
			}
		}

		#endregion // ResumeNotificationProcessing 

		#region PerformActionPartial

		// SSP 1/17/05 BR01753
		//


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal object PerformActionPartial( UltraCalcAction action, object data )
		{
			switch ( action )
			{
				case UltraCalcAction.AddReferenceToRecalcChain:
					{
						IUltraCalcReference reference = (IUltraCalcReference)data;
						if ( null == reference.Formula )
							throw new UltraCalcException( "Can't add a reference without a formula to the recalc chain." );

						this.DirtyParents( reference, true );
						return null;
					}
				default:
					{
						Debug.Assert( false, "Unknown action." );
						return null;
					}
			}
		}

		#endregion // PerformActionPartial 

		#region DirtyAllFormulas






		internal void DirtyAllFormulas( )
		{
			foreach ( ReferenceHolder rh in this.GetFormulas( ) )
				this.recalcChain.Add( rh.Reference );
		}

		#endregion // DirtyAllFormulas

		#region NotifyTopologicalChange

		private int inNotifyTopologicalChange = -1;
		internal void NotifyTopologicalChange(IUltraCalcReference reference, ReferenceActionCode action)
		{
			// SSP 10/15/04
			// Added a way to suspend processing of calc notifications because if there are a lot of
			// such notifications being sent from the grid then it may take a very long time to process
			// them. Workaround would be to suspend notification processing and then dirty all the 
			// formulas so every thing gets recalculated.
			//
			if ( !this.IsNotificationProcessingSuspended ) 
			{
				try
				{
					this.inNotifyTopologicalChange++;

					QueueInfo item = new QueueInfo( reference, action );

					// Process the row deletions synchronously because if we queued them then by the time
					// we go to process them the row reference is already disposed off.
					//
					// If Delete gets called recursively then enque the message.
					//
					if ( 0 == this.inNotifyTopologicalChange && ReferenceActionCode.Delete == action )
						this.ProcessTopologicalEvent( item );
					else
                    {
                        // MRS 2/4/2008 - BR30242
                        this.DebugOutputEnqueueItem(item);

                        this.eventQueue.Enqueue(item);
                    }
				}
				finally
				{
					this.inNotifyTopologicalChange--;
				}
			}
		}

		#endregion //NotifyTopologicalChange

		#region NotifyValueChanged

		// MD 3/16/09 - TFS14252
		// Added a return type of bool to indicate whether any parents were actually dirtied.
		//internal void NotifyValueChanged(IUltraCalcReference reference)


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		internal bool NotifyValueChanged( IUltraCalcReference reference )
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
			if ( !this.IsNotificationProcessingSuspended ) 
			{
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
			}

			// MD 3/16/09 - TFS14252
			// The method now has a return value.
			return returnValue;
		}

		#endregion //NotifyValueChanged

		#region RaiseRecalcValueEvent

		internal void RaiseRecalcValueEvent( IUltraCalcReference reference )
		{
			// SSP 7/13/11 - XamCalculationManager
			// 
			//this.OnRecalcValue( new UltraCalcReferenceEventArgs( reference ) );
			this.OnRecalcValue( reference );
		}

		#endregion //RaiseRecalcValueEvent 

		//XAML_TODO


#region Infragistics Source Cleanup (Region)




























#endregion // Infragistics Source Cleanup (Region)


		// MD 7/22/08
		// Found while implementing Excel formula solving
		// Created helper method from some code in Recalc so it could be used in other places.
		#region ClearDirtyChainAndDoTopologicalSort

		// MD 8/20/08 - Excel formula solving
		// Added an extra parameter which indicates whether the initialize formulas for a new recalc operation.
		//internal void ClearDirtyChainAndDoTopologicalSort()
		internal void ClearDirtyChainAndDoTopologicalSort( bool isStartOfRecalcOperation )
		{
			// Something on the dirty chain? If so ClearDirtyChain.
			ClearDirtyChain();

			// Need to sort the recalc chain?
			if ( doTopologicalSort )
				TopologicalSort();
			
			// MD 8/20/08 - Excel formula solving
			if ( isStartOfRecalcOperation )
			{
				// MD 10/25/10 - TFS36696
				// We don't need to reset the iteration count and dependancy sort order for each formula. We just need to do it for the formulas in circular references,
				// since those are the ones that will have their  iteration count and dependancy sort order changed.
				//for ( int i = 0; i < this.recalcChain.Count; i++ )
				//{
				//    UltraCalcFormulaBase formula = this.recalcChain[ i ].Formula as UltraCalcFormulaBase;
				//
				//    if ( formula != null )
				//        formula.OnBeforeRecalc();
				//}
				foreach (KeyValuePair<IUltraCalcReference, object> entry in this.circularReferences)
				{
					UltraCalcFormulaBase formula = entry.Key.Formula as UltraCalcFormulaBase;

					if (formula != null)
						formula.OnBeforeRecalc();
				}
			}
		} 

		#endregion ClearDirtyChainAndDoTopologicalSort

		// MD 7/23/08 - Excel formula solving
		#region DirtyAlwaysDirtyList






		internal void DirtyAlwaysDirtyList()
		{
			// MD 11/1/10 - TFS56976
			// Wrapped this foreach loop in an if statement. There's no point to creating and disposing an enumerator when there are no items.
			if (this.alwaysDirtyList.Count > 0)
			{
				foreach (IUltraCalcReference alwaysDirtyItem in this.alwaysDirtyList)
					this.DirtyParents(alwaysDirtyItem, true);
			}

			// MD 8/20/08 - Excel formula solving
			// Circular reference formulas are always dirty, so dirty them as well.
			// MD 11/1/10 - TFS56976
			// Wrapped this foreach loop in an if statement. There's no point to creating and disposing an enumerator when there are no items.
			if (this.circularReferences.Count > 0)
			{
				foreach (IUltraCalcReference reference in this.circularReferences.Keys)
					this.DirtyParents(reference, true);
			}
		} 

		#endregion DirtyAlwaysDirtyList

		#endregion // Private/Internal Methods

		#region Public Methods

		#region Add Function/Library



#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)


		/// <summary>
		/// Registers an assembly containing <see cref="UltraCalcFunction"/> derived types.
		/// </summary>
		/// <param name="assembly">Loaded assembly to register</param>
		/// <returns>Returns true if the assembly was registered successfully, else false if the registration failed</returns>
		/// <remarks>
		/// <p class="body">
		/// All types within the registered assembly are enumerated and any that derive from <see cref="UltraCalcFunction"/> class are added to the list of available formula functions
		/// </p>
		/// </remarks>
		/// <seealso cref="AddFunction"/>
		public bool AddFunctionLibrary(Assembly assembly)
		{
			return this.functionFactory.AddLibrary(assembly);
		}

		/// <summary>
		/// Registers a single <see cref="UltraCalcFunction"/>
		/// </summary>
		/// <param name="function">User defined function instance to register</param>
		/// <returns>Returns true if the type was registered successfully, else false if the registration failed</returns>
		/// <remarks>
		/// <p class="body">
		/// Users can build custom functions used in formulas by sub-classing the <see cref="UltraCalcFunction"/> class.  
		/// Once the derived class is instaniated it must be registered by using the <b>AddFunction</b> method before being avilable and referenced by a formulas.
		/// Users can build a library of functions packaged in an assembly and register all the functions within the assembly by using the <b>AddFunctionLibrary</b> method.
		/// </p>
		/// </remarks>        
		public bool AddFunction(UltraCalcFunction function)
		{
			return functionFactory.Add(function);
		}

		#endregion // Add Function/Library

		#region Add/Delete Formula

		/// <summary>
		/// Adds a formula into the calculation network.
		/// </summary>
		/// <param name="formula">Formula instance containing compiled formula string to add into the calculation network</param>
		/// <remarks>
		/// <p class="body">
		/// Formulas are entered into the calculation network by first compiling the formula string using the <b>Compile</b> method on the <b>UltraCalcFormula</b> class, then passing the <b>UltraCalcFormula</b> instance to the <b>AddFormula</b> method.
		/// </p>
		/// </remarks>
		public void AddFormula(IUltraCalcFormula formula)
		{
            // MRS 2/4/2008 - BR30242
            this.DebugOutputAddFormula(formula);

			this.eventQueue.Enqueue(new QueueInfo(formula.BaseReference, ReferenceActionCode.AddFormula, formula));
		}

		/// <summary>
		/// Deletes a formula from the calculation network
		/// </summary>
		/// <param name="formula">Formula instance to be removed</param>
		/// <remarks>
		/// <p class="body">
		/// References to formulas that are deleted are recalculated.
		/// </p>
		/// </remarks>
		/// <seealso cref="ProcessAddFormula"/>
		/// <seealso cref="UltraCalcFormulaBase"/>
		public void DeleteFormula(IUltraCalcFormula formula)
        {
            // MRS 2/4/2008 - BR30242
            this.DebugOutputDeleteFormula(formula);

			this.eventQueue.Enqueue(new QueueInfo(formula.BaseReference, ReferenceActionCode.DeleteFormula, formula));
		}

		#endregion //Add/Delete Formula		

		#region Recalc

		/// <summary>
		/// Performs a time-based recalculation
		/// </summary>
		/// <param name="ticks">Denotes the number of 100-nanosecond intervals used to perform time-based partial recalculation cycles</param>
		/// <returns>Returns true if there are still formulas to recalculate or false if recalculation process is complete</returns>
		/// <remarks>
		/// <p class="body">
		/// This method performs a partial recalculation cycle, yielding after the given number of ticks has expired.  This method should be repeately
		/// called until it returns false denoting all dirty formulas have been recalculated.
		/// <p></p>
        /// <seealso cref="Recalc(long)"/> 
		/// </p>
		/// </remarks>
		public bool Recalc(long ticks)
		{
			// MD 8/20/08 - Excel formula solving
			// Moved all code to the new overload
			return this.Recalc( ticks, true );
		}

		// MD 8/20/08 - Excel formula solving
		// Added new parameter to indicate whether this is the start if a new recalc operation.
		internal bool Recalc( long ticks, bool isStartOfRecalcOperation )
		{
			bool hasMoreToProcess;

			// SSP 1/18/05 BR01753
			// Enclosed the following block of code in do-while loop. If ticks is -1 then we need to
			// ensure that if topological events were fired during recalc, then we clean the topological
			// events and calculate everything before returning.
			//
			do
			{
				// Process any pending topological events and the time allocated is used up without processing
				// all the events then return true.
				//
				// MD 8/21/08 - Excel formula solving - Performance
				// Only clear the topological event queue when it has items on it.
				if ( this.eventQueue.Count > 0 )
				{
					if ( ! this.CleanTopologicalEventQueue( ticks ) )
						return true;
				}

				hasMoreToProcess = this.HasMoreToCalc;
				if ( hasMoreToProcess )
				{
					//Debug.WriteLine("UltraCalcWindowsForm: Do Recalc");

					// Get current TickCount to compute timeslice
					long recalcTicks = DateTime.Now.Ticks;

					// MD 7/22/08
					// Found while implementing Excel formula solving
					// This has been moved to a helper method so it can be used in other places.
					//// Something on the dirty chain? If so ClearDirtyChain.
					//ClearDirtyChain();
					//
					//// Need to sort the recalc chain?
					//if (doTopologicalSort)
					//    TopologicalSort();
					// MD 8/20/08 - Excel formula solving
					// Pass off the new parameter
					//this.ClearDirtyChainAndDoTopologicalSort();
					this.ClearDirtyChainAndDoTopologicalSort( isStartOfRecalcOperation );
				
					// Still have recalc objects and haven't exceeded our timeslice?
					//while (recalcChain.Count > 0 && (((DateTime.Now.Ticks - Ticks) < recalcTaskTicks) || syncRecalc))
					// SSP 10/8/04
					//
					//while (eventQueue.Count == 0 && recalcChain.Count > 0 && (ticks == -1 || ((DateTime.Now.Ticks - recalcTicks) < ticks)))
					while (eventQueue.Count == 0 && this.HasMoreToCalc && (ticks == -1 || ((DateTime.Now.Ticks - recalcTicks) < ticks)))
						// Evaluate the current recalc object
						Evaluate(ticks);

					// Notify listners when recalc is complete if we don't have anything on the recalc chain or we have a circular reference
					// SSP 10/8/04
					//
					//if (recalcChain.Count == 0)
					hasMoreToProcess = this.HasMoreToCalc || this.eventQueue.Count > 0;
				}
			}
			while ( -1 == ticks && hasMoreToProcess );

			// Return indicator denoting whether there's still more work to do
			// SSP 10/8/04
			//
			//return (recalcChain.Count > 0 && circularReference == null);
			return hasMoreToProcess;
		}

		/// <summary>
		/// Performs a synchronous recalculation
		/// </summary>
		/// <returns>Returns true if the reference is dirty, else false</returns>
		/// <p></p>
        /// <seealso cref="Recalc(long)"/> 
		/// <remarks>
		/// <p class="body">
		/// This method recalculates all dirty formulas before returning.
		/// </p>
		/// </remarks>
		public void Recalc()
		{
			Recalc(-1);
		}

		#endregion //Recalc

		#region IsDirty

		/// <summary>
		/// Checks to see if a reference is dirty, and optionally waits for it to be recalculated
		/// </summary>
		/// <param name="reference">Reference whose dirty state is requested</param>
		/// <param name="wait">Denotes whether caller wants to wait until the reference is recalculated</param>
		/// <returns>True if the reference is dirty</returns>
		public bool IsDirty(IUltraCalcReference reference, bool wait)
		{
			// MD 8/20/08 - Excel formula solving
			// Moved all code to the new overload
			return this.IsDirty( reference, wait, true );
		}

		// MD 8/20/08 - Excel formula solving
		// Added new parameter to indicate whether this will start a new recalc operation
		internal bool IsDirty( IUltraCalcReference reference, bool wait, bool isStartOfRecalcOperation )
		{
			// MD 7/15/08
			// Found while implementing Excel formula solving
			// We should be processing any pending notification before checking to see if the reference is dirty.
			// MD 8/17/08 - Excel formula solving
			// Performance - the event queue should only be cleaned when there are items in the event queue.
			//this.CleanTopologicalEventQueue( -1 );
			if ( this.eventQueue.Count > 0 )
				this.CleanTopologicalEventQueue( -1 );

			// MD 7/22/08
			// Found while implementing Excel formula solving
			// The recalcChain may be incomplete at this point. Moved some logic from Recalc into a helper and called
			// it from here so we complete the recalcChain before searching it for the passed in reference.
			// MD 8/17/08 - Excel formula solving
			// Performance - We should only clear and sort the dirty chain if the reclac chain has items in it.
			//this.ClearDirtyChainAndDoTopologicalSort();
			if ( this.recalcChain.Count > 0 || this.circularReference != null )
			{
				// MD 8/20/08 - Excel formula solving
				// Pass off the new parameter
				//this.ClearDirtyChainAndDoTopologicalSort();
				this.ClearDirtyChainAndDoTopologicalSort( isStartOfRecalcOperation );
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
				while (this.IsDirty(reference, false, false) && this.Recalc(UltraCalcEngine.RecalcTaskTicks, false)) ; 

				return false;
			} 
			else
			{
				// SSP 10/7/04
				// Instead of checking the flag call IsRecalcListed method.
				//
				return this.recalcChain.IsRecalcListed( reference )
						|| null != this.pendingCalcInfo && this.pendingCalcInfo.IsBeingEvaluated( reference ) 
					;
			}
		}

		#endregion //IsDirty

		#region Dump

		/// <summary>
		/// Outputs information about the current state of the calculation engine.
		/// </summary>
		[Conditional("DEBUG")]
		public void Dump()
		{
			Debug.WriteLine( "+===================================================================================================================+" );
			if ( null != this.pendingCalcInfo )
				this.pendingCalcInfo.Dump();
			else
				Debug.WriteLine( "No PendingCalcReference" ); 

			Debug.WriteLine("+-------------------------------------------------------------------------------------------------------------------+");
			recalcChain.Dump();
		}

		#endregion //Dump

		#endregion // Public Methods

		#region Commented Out Old Code

		// SSP 10/7/04
		// Commented out FoundFormula method because it's not being used anywhere.
		//
		
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


		// SSP 10/8/04
		// Now the RecalcDeferred is being set through the VerifyAncestorMap logic.
		//
		
#region Infragistics Source Cleanup (Region)




























































#endregion // Infragistics Source Cleanup (Region)

		
		
#region Infragistics Source Cleanup (Region)





















#endregion // Infragistics Source Cleanup (Region)


		#endregion // Commented Out Old Code
        
        // MRS 2/4/2008 - BR30242
        #region DEBUG_OUTPUT_TOPOLOGY_INFO

        #region DebugOutputEnqueueItem
        [Conditional("DEBUG_OUTPUT_TOPOLOGY_INFO")]
        private void DebugOutputEnqueueItem(QueueInfo item)
        {
            Debug.WriteLine("ENQUEUE Item");
            Debug.WriteLine(string.Format("\tAction: {0}", item.Action.ToString()));
            Debug.WriteLine(string.Format("\tReference: {0}", item.Reference.ToString()));
            Debug.WriteLine(string.Format("\tFormula: {0}", item.Formula != null ? item.Formula.ToString() : null));
        }

        #endregion DebugOutputEnqueueItem

        #region DebugOutputProcessQueueItem
        [Conditional("DEBUG_OUTPUT_TOPOLOGY_INFO")]
        private void DebugOutputProcessQueueItem(QueueInfo item)
        {
            Debug.WriteLine("PROCESS Item");
            Debug.WriteLine(string.Format("\tAction: {0}", item.Action.ToString()));
            Debug.WriteLine(string.Format("\tReference: {0}", item.Reference.ToString()));
            Debug.WriteLine(string.Format("\tFormula: {0}", item.Formula != null ? item.Formula.ToString() : null));
        }

        #endregion DebugOutputProcessQueueItem

        #region DebugOutputAddFormula
        [Conditional("DEBUG_OUTPUT_TOPOLOGY_INFO")]
        private void DebugOutputAddFormula(IUltraCalcFormula formula)
        {
            Debug.WriteLine("ADD Formula");
            Debug.WriteLine(string.Format("\tFormula: {0}", formula.FormulaString));
            Debug.WriteLine(string.Format("\tBase Reference: {0}", formula.BaseReference.ToString()));
        }

        #endregion DebugOutputAddFormula

        #region DebugOutputDeleteFormula
        [Conditional("DEBUG_OUTPUT_TOPOLOGY_INFO")]
        private void DebugOutputDeleteFormula(IUltraCalcFormula formula)
        {
            Debug.WriteLine("DELETE Formula");
            Debug.WriteLine(string.Format("\tFormula: {0}", formula.FormulaString));
            Debug.WriteLine(string.Format("\tBase Reference: {0}", formula.BaseReference.ToString()));
        }

        #endregion DebugOutputDeleteFormula

        [Conditional("DEBUG_OUTPUT_TOPOLOGY_INFO")]
        private void DebugOutputEvaluate()
        {
            Debug.WriteLine("EVALUATE");
        }

        #endregion //DEBUG_OUTPUT_TOPOLOGY_INFO

		// MD 7/25/08 - Excel formula solving
		#region GetResolvedReference






		internal static IUltraCalcReference GetResolvedReference( IUltraCalcReference reference )
		{
			// MD 8/26/08 - BR35804
			// Moved all code to the new overload
			return UltraCalcEngine.GetResolvedReference( reference, false );
		}

		// MD 8/26/08 - BR35804
		// Added a new overload so the caller could specify whether to unwrap all reference wrappers or only UCReference wrappers.





		internal static IUltraCalcReference GetResolvedReference( IUltraCalcReference reference, bool unwrapOnlyUCReferences )
		{
			if ( reference == null )
				return null;

			// MD 8/26/08 - BR35804
			// Certain wrappers contain inportant information for formula solving and shouldn't be unwrapped.
			if ( unwrapOnlyUCReferences == false )
			{
				RefUnAnchored refUnAnchored = reference as RefUnAnchored;

				if ( refUnAnchored != null )
					return UltraCalcEngine.GetResolvedReference( refUnAnchored.WrappedReference ); 

				FormulaReference formulaReference = reference as FormulaReference;

				if ( formulaReference != null )
					return UltraCalcEngine.GetResolvedReference( formulaReference.UnderlyingReference );
			}

			UCReference ucReference = reference as UCReference;

			if ( ucReference != null && ucReference.IsConnected )
				return UltraCalcEngine.GetResolvedReference( ucReference.Reference, unwrapOnlyUCReferences );

			return reference;
		}

		#endregion GetResolvedReference

		// MD 8/20/08 - Excel formula solving
		#region AfterFormulaEvaluationCompleted

		/// <summary>
		/// Occurs after a single evaluation of a formula has completed.
		/// </summary>
		private void AfterFormulaEvaluationCompleted( IUltraCalcReference reference, UltraCalcFormulaBase referenceFormula, UltraCalcValue evaluatedValue )
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
				if (this.IterativeCalculationsEnabled &&
					referenceFormula.IterationCount < this.MaxRecursionIterations)
				{
					bool isUnderMaxChangeInIteration = false;

					// If this is the second iteration or more for the formula in this round of reclaculations, check to see if the 
					// newly calculated value differs from the last calculated value by more than the threshhold needed to continue
					// iterative calculations. If not, don't continue iterations.
					double currentValue;
					double nextValue;
					if (referenceFormula.IterationCount > 1 && reference.Value.ToDouble(out currentValue) && evaluatedValue.ToDouble(out nextValue))
					{
						if (Math.Abs(nextValue - currentValue) < Math.Abs(this.MaxChangeInIteration))
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
							IUltraCalcReference lastReference = this.recalcChain[this.recalcChain.Count - 1];
							UltraCalcFormulaBase lastFormula = lastReference.Formula as UltraCalcFormulaBase;

							// Make its dependancy sort order temporarily greater than the last formula's so it will be at the end of the chain.
							if (lastFormula != null)
								referenceFormula.SetDependancySortNumberForCircularReference(lastFormula.DependancySortNumber + 1);
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


		// SSP 7/13/11 - XamCalculationManager
		// 
		#region ISupportPropertyChangeNotifications interface implementation

		private PropertyChangeListenerList _listeners = new PropertyChangeListenerList( );

		public void AddListener( ITypedPropertyChangeListener<object, string> listener, bool useWeakReference )
		{
			_listeners.Add( listener, useWeakReference );
		}

		public void RemoveListener( ITypedPropertyChangeListener<object, string> listener )
		{
			_listeners.Remove( listener );
		} 

		#endregion // ISupportPropertyChangeNotifications interface implementation

	}

	#endregion // UltraCalcEngine Class





#region Infragistics Source Cleanup (Region)

















































































































































#endregion // Infragistics Source Cleanup (Region)


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