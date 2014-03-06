using System;
using System.Collections;
using System.Globalization;
using System.Diagnostics;
using System.Collections.Generic;


using UltraCalcFunction = Infragistics.Calculations.Engine.CalculationFunction;
using UltraCalcNumberStack = Infragistics.Calculations.Engine.CalculationNumberStack;
using UltraCalcValue = Infragistics.Calculations.Engine.CalculationValue;
using IUltraCalcReference = Infragistics.Calculations.Engine.ICalculationReference;
using IUltraCalcFormula = Infragistics.Calculations.Engine.ICalculationFormula;
using UltraCalcErrorValue = Infragistics.Calculations.Engine.CalculationErrorValue;
using IUltraCalcReferenceCollection = Infragistics.Calculations.Engine.ICalculationReferenceCollection;



namespace Infragistics.Calculations.Engine





{
	/// <summary>
	/// Provides methods and properties that manage a collection of <b>IUltraCalcFormulaReference</b> objects.
	/// </summary>
	internal class UltraCalcFormulaReferenceCollection : IUltraCalcReferenceCollection
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		public UltraCalcFormulaReferenceCollection()
		{
            tokenArray = new List<IUltraCalcReference>();
		}

		/// <summary>
		/// Constructor that accepts a collection of formula tokens used to access the references in a formula.
		/// </summary>
		/// <param name="formula">The formula which owns the collection.</param>
		// MD 8/4/08 - Excel formula solving
		// The constructor needs more info now, so we need a reference to the formula instance, not just its tokens
		//public UltraCalcFormulaReferenceCollection(UltraCalcFormulaTokenCollection tokens) : this()
		// MD 8/17/08 - Excel formula solving
		// Refactoring - Renamed UltraCalcFormula to UltraCalcFormulaBase
		//public UltraCalcFormulaReferenceCollection( UltraCalcFormula formula )
		public UltraCalcFormulaReferenceCollection( UltraCalcFormulaBase formula )
			: this()
		{
			// MD 8/4/08 - Excel formula solving
			// The tokens are not passed directly to the constructor anymore
			//if (tokens != null) 
			if ( formula.Tokens != null ) 
			{
				// MD 7/30/08 - Excel formula solving
				// We can't just add all references to the static references. Some references will never be dereferenced
				// and shouldn't be added. Instead, we need to determine which references are definately going to be 
				// dereferenced and only add those.
				//foreach (UltraCalcFormulaToken token in tokens) 
				//{
				//    if (token.Type == UltraCalcFormulaTokenType.Value && ((UltraCalcValueToken)token).Value.IsReference)
				//        //Add(((UltraCalcValue)Token).ToReference(CultureInfo.InvariantCulture));
				//        this.Add((IUltraCalcReference)((UltraCalcValueToken)token).Value.Value);
				//}

				Stack<UltraCalcFormulaToken> tokenStack = new Stack<UltraCalcFormulaToken>();

				// Loop over the token collection applying operators
				// MD 8/4/08 - Excel formula solving
				// The tokens are not passed directly to the constructor anymore
				//foreach ( UltraCalcFormulaToken token in tokens )
				foreach ( UltraCalcFormulaToken token in formula.Tokens )
				{
					// Value tokens should just be pushed on the stack
					if ( token.Type == UltraCalcFormulaTokenType.Value )
					{
						tokenStack.Push( token );
						continue;
					}

					// Other tokens are assumed to be function tokens and should pop their arguments off the token stack.
					UltraCalcFunctionToken functionToken = (UltraCalcFunctionToken)token;

					// The arguments are in reverse order comming off the stack, so start the argument count at the end.
					for ( int parameterIndex = functionToken.ArgumentCount - 1; parameterIndex >= 0; parameterIndex-- )
					{
						this.TryAdd( formula, tokenStack.Pop());
					}

					// Push the function token on the stack so it can be used by the next function
					tokenStack.Push( functionToken );
				}

				// After all tokens are processed, there should only be one token left on the stack.
				Debug.Assert( tokenStack.Count == 1 );

				// Add all remaining references on the stack to the static references collection.
				while ( tokenStack.Count > 0 )
				{
					this.TryAdd( formula, tokenStack.Pop());
				}
			}
		}

		/// <summary>
		/// Adds a reference to the collection.
		/// </summary>
		/// <param name="reference"><b>IUltraCalcReference</b> instance to add to collection.</param>
		public void Add(IUltraCalcReference reference)
		{
			this.tokenArray.Add(reference);
		}

		#region Implementation of ICollection
		/// <summary>
		/// Copys the collection to an array.
		/// </summary>
		/// <param name="array">Array used for the desitnation of the copy.</param>
		/// <param name="index">The zero-based index in array at which copying begins.</param>
		public void CopyTo(System.Array array, int index)
		{
			this.tokenArray.CopyTo(array as IUltraCalcReference[], index);	
		}

		/// <summary>
		/// Denotes whether the collection is thread-safe.
		/// </summary>
		public bool IsSynchronized
		{
			get
			{
				return CalcManagerUtilities.IsSynchronized(this.tokenArray);
			}
		}
	
		/// <summary>
		/// Returns a number of elements in the collection.
		/// </summary>
		public int Count
		{
			get
			{
				return this.tokenArray.Count;
			}
		}

		/// <summary>
		/// Returns an object that can be used to synchronize access to the collection.
		/// </summary>
		public object SyncRoot
		{
			get
			{
                return CalcManagerUtilities.SyncRoot(this.tokenArray);
			}
		}
		#endregion

		#region Implementation of IEnumerable
		/// <summary>
		/// Returns the collection enumerator.
		/// </summary>
		/// <returns>Collection enumerator.</returns>
		public System.Collections.IEnumerator GetEnumerator()
		{
			return this.tokenArray.GetEnumerator();
		}
		#endregion

		/// <summary>
		/// Underlying collection object managing the token collection.
		/// </summary>
		private List<IUltraCalcReference> tokenArray;

		// MD 7/25/08 - Excel formula solving
		#region Contains

		internal bool Contains( IUltraCalcReference reference, bool checkEquality )
		{
			foreach ( IUltraCalcReference existingRef in this.tokenArray )
			{
				if ( checkEquality )
				{
					if ( reference.Equals( existingRef ) )
						return true;

					Debug.Assert(
						UltraCalcEngine.GetResolvedReference( reference ).Equals( UltraCalcEngine.GetResolvedReference( existingRef ) ) == false,
						"The references should be resolved before they are compared in here." );
				}
				else
				{
					if ( reference.ContainsReference( existingRef ) )
						return true;
				}
			}

			return false;
		} 

		#endregion Contains

		// MD 7/30/08 - Excel formula solving
		#region Remove

		internal void Remove( IUltraCalcReference reference )
		{
			for ( int i = 0; i < this.tokenArray.Count; i++ )
			{
				IUltraCalcReference existingRef = (IUltraCalcReference)this.tokenArray[ i ];

				if ( existingRef.Equals( reference ) )
				{
					this.tokenArray.RemoveAt( i );
					return;
				}
			}
		}  

		#endregion Remove

		// MD 8/18/08 - Excel formula solving
		#region TryAdd

		private void TryAdd( UltraCalcFormulaBase formula, UltraCalcFormulaToken currentArgument)
		{
			// Only value tokens can be static references
			if ( currentArgument.Type != UltraCalcFormulaTokenType.Value )
				return;

			UltraCalcValueToken valueArgument = (UltraCalcValueToken)currentArgument;

			// Only reference values can be added as static tokens, so ignore it if it is not a reference.
			if ( valueArgument.Value.IsReference == false )
				return;

			IUltraCalcReference reference = (IUltraCalcReference)valueArgument.Value.Value;

			// Add the reference to the static references collection
			this.Add( reference );

			return;
		} 

		#endregion TryAdd
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