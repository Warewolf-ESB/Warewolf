using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;


using System.Linq;





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
using ExcelResources = SR;



namespace Infragistics.Calculations.Engine





{

	#region RecalcChainSortComparer 
	





	internal class RecalcChainSortComparer : IComparer
	{
		int IComparer.Compare( object xVal, object yVal )
		{
			// MD 8/17/08 - Excel formula solving
			// Refactoring - Renamed UltraCalcFormula to UltraCalcFormulaBase
			//UltraCalcFormula xFormula = xVal is IUltraCalcReference
			//    ? (UltraCalcFormula)((IUltraCalcReference)xVal).Formula
			//    : null;
			//
			//UltraCalcFormula yFormula = yVal is IUltraCalcReference
			//    ? (UltraCalcFormula)((IUltraCalcReference)yVal).Formula
			//    : null;				
			UltraCalcFormulaBase xFormula = xVal is IUltraCalcReference
				? (UltraCalcFormulaBase)( (IUltraCalcReference)xVal ).Formula
				: null;

			UltraCalcFormulaBase yFormula = yVal is IUltraCalcReference
				? (UltraCalcFormulaBase)( (IUltraCalcReference)yVal ).Formula
				: null;				

		//	Debug.Assert( null != xFormula && null != yFormula );

			// Sort numbers are always greater than 0 so this will take care of null
			// references and references with null formulas as well although formulas
			// should not be null.
			//
			int xSortNumber = null != xFormula ? xFormula.DependancySortNumber : -1;
			int ySortNumber = null != yFormula ? yFormula.DependancySortNumber : -1;

			return xSortNumber < ySortNumber ? -1 : ( xSortNumber > ySortNumber ? 1 : 0 );
		}
	}

	#endregion // RecalcChainSortComparer 

	#region TokenInfo Class



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

	internal class TokenInfo
	{
		#region ScopeType Enum



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal enum ScopeType
		{
			RelativeIndex,
			Other
		}

		#endregion // ScopeType Enum 

		#region Private Vars

		private ReferenceHolder referenceHolder = null;
		private ScopeType scope = ScopeType.RelativeIndex;
		private int scopeIndex = 0; 

		#endregion // Private Vars

		#region Constructor






		internal TokenInfo( ReferenceHolder referenceHolder
			, ScopeType scope, int scopeIndex  
 )
		{
			if ( null == referenceHolder || null == referenceHolder.Reference )
			{
				Debug.Assert( false );
				throw new ArgumentNullException( );
			}
				
			this.referenceHolder = referenceHolder;
			this.scope = scope;
			this.scopeIndex = scopeIndex; 
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		// SSP 9/4/08 BR31006
		// Added referenceHolder parameter.
		//
		//internal TokenInfo( IUltraCalcReference reference, TokenInfo deltaScope )
		private TokenInfo( ReferenceHolder referenceHolder, IUltraCalcReference reference, TokenInfo deltaScope )
		{
			Debug.Assert( null != reference && null != deltaScope );
			if ( null == reference || null == deltaScope )
				throw new ArgumentNullException( );

			// SSP 9/4/08 BR31006
			// Added referenceHolder parameter. Use that instead of creating a new one from reference.
			//
			//this.referenceHolder = new ReferenceHolder( reference );
			this.referenceHolder = referenceHolder;

			TokenInfo.GetScopeInfo( reference, out this.scope, out this.scopeIndex );

			if ( TokenInfo.ScopeType.RelativeIndex == this.scope
				&& TokenInfo.ScopeType.RelativeIndex == deltaScope.Scope
				// The references must be siblings otherwise circular references are not
				// allowed (for example when a column formula refers to a parent band's
				// column relatively or otherway around as well).
				//
				&& this.Reference.IsSiblingReference( deltaScope.Reference ) )
			{
				this.scopeIndex += deltaScope.ScopeIndex;
			}
			else 
			{
				this.scope = TokenInfo.ScopeType.Other;
				this.scopeIndex = 0; 
			}
		}

		#endregion // Constructor

		#region Scope

		internal TokenInfo.ScopeType Scope
		{
			get
			{
				return this.scope;
			}
		}

		#endregion // Scope 

		#region ScopeIndex

		internal int ScopeIndex
		{
			get
			{
				return TokenInfo.ScopeType.Other != this.scope ? this.scopeIndex : 0;
			}
		}

		#endregion // ScopeIndex 

		#region ReferenceHolder

		internal ReferenceHolder ReferenceHolder
		{
			get
			{
				return this.referenceHolder;
			}
		}

		#endregion // ReferenceHolder

		#region Reference
			





		internal IUltraCalcReference Reference
		{
			get
			{
				return this.referenceHolder.Reference;
			}
		}

		#endregion // Reference

		#region Formula
			





		// MD 8/17/08 - Excel formula solving
		// Refactoring - Renamed UltraCalcFormula to UltraCalcFormulaBase
		//internal UltraCalcFormula Formula
		internal UltraCalcFormulaBase Formula
		{
			get
			{
				// MD 8/17/08 - Excel formula solving
				// Refactoring - Renamed UltraCalcFormula to UltraCalcFormulaBase
				//return (UltraCalcFormula)this.Reference.Formula;
				// SSP 9/4/08 BR31006 - Optimization
				// Get the Formula off of the reference holder which caches the formula.
				// 
				//return (UltraCalcFormulaBase)this.Reference.Formula;
				return this.referenceHolder.Formula;
			}
		}

		#endregion // Formula

		#region GetHashCode

		public override int GetHashCode( )
		{
			return 
				(int)this.Scope ^ this.ScopeIndex ^
				this.referenceHolder.GetHashCode( );
		}

		#endregion // GetHashCode

		#region Equals

		public override bool Equals( object obj )
		{
			TokenInfo t = obj as TokenInfo;
			return null != t 
				&& t.Scope == this.Scope
				&& t.ScopeIndex == this.ScopeIndex 
				&& this.referenceHolder.Equals( t.referenceHolder );
		}

		#endregion // Equals

		#region IntersectsWith
			


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal bool IntersectsWith( TokenInfo token )
		{
			return this.ReferenceHolder.Equals( token.ReferenceHolder ) 
				&& ( TokenInfo.ScopeType.RelativeIndex != this.Scope 
				|| TokenInfo.ScopeType.RelativeIndex != token.Scope
				|| this.ScopeIndex == token.ScopeIndex )
				;
		}

		#endregion // IntersectsWith

		#region GetScopeInfo

		internal static void GetScopeInfo( IUltraCalcReference reference, out TokenInfo.ScopeType scope, out int scopeIndex )
		{
			scope = TokenInfo.ScopeType.Other;
			scopeIndex = 0;

			RefParser rp = null;
			try
			{
				if ( reference is RefBase )
					rp = ((RefBase)reference).ParsedReference;

				if ( null == rp )
					rp = new RefParser( reference.AbsoluteName );
			}
			catch ( Exception exc )
			{
				Debug.Assert( false, exc.Message );
			}

			Debug.Assert( null != rp && rp.TupleCount > 0, "Invalid parsed reference." );

			if ( null != rp && rp.TupleCount > 0 )
			{
				RefTuple tuple = rp.LastTuple;
				if ( RefTuple.RefScope.Any == tuple.Scope && null != rp.NextToLastTuple )
					tuple = rp.NextToLastTuple;

				if ( RefTuple.RefScope.RelativeIndex == tuple.Scope )
				{
					scope = TokenInfo.ScopeType.RelativeIndex;
					scopeIndex = tuple.ScopeIndex;
				}
				// Scope of Any is like RelativeIndex scope with the relative index of 0.
				// We are doing this for convenience.
				//
				else  
					if ( RefTuple.RefScope.Any == tuple.Scope )
				{
					scope = TokenInfo.ScopeType.RelativeIndex;
					scopeIndex = 0;
				}
			}
		}

		#endregion // GetScopeInfo

		#region TokenSortComparer Class

		internal class TokenSortComparer : IComparer
		{
			internal bool circularityDetected = false;

			// SSP 6/27/07 - BR24314 - Optimizations
			// 
			private AncestorMapCache cache;

			// SSP 6/27/07 - BR24314 - Optimizations
			// Added constructor.
			// 
			internal TokenSortComparer( AncestorMapCache cache )
			{
				this.cache = cache;
			}

			public int Compare( object xVal, object yVal )
			{
				TokenInfo xt = xVal as TokenInfo;
				TokenInfo yt = yVal as TokenInfo;

				if ( xt == yt )
					return 0;
				else if ( null == xt )
					return -1;
				else if ( null == yt )
					return 1;

				// If xt depends on yt then xt should appear after yt in the recalc chain.
				//
				// SSP 6/27/07 - BR24314 - Optimizations
				// Added cache member variable.
				// 
				//if ( TokenInfo.IsDependant( xt, yt ) )
				if ( TokenInfo.IsDependant( this.cache, xt, yt ) )
				{
					// We should not get here if there were a circularity.
					//
					// SSP 6/27/07 - BR24314 - Optimizations
					// Added cache member variable.
					// 
					//this.circularityDetected = this.circularityDetected || TokenInfo.IsDependant( yt, xt );
					this.circularityDetected = this.circularityDetected || TokenInfo.IsDependant( this.cache, yt, xt );
					Debug.Assert( ! circularityDetected, "Circularity detected in sort !" );

					return 1;
				}
				// SSP 6/27/07 - BR24314 - Optimizations
				// Added cache member variable.
				// 
				//else if ( TokenInfo.IsDependant( yt, xt ) )
				else if ( TokenInfo.IsDependant( this.cache, yt, xt ) )
				{
					return -1;
				}
				else
				{
					// If xt and yt are independent then cause sibling formulas to group
					// together.
					//
					return xt.Formula.SiblingNumber.CompareTo( yt.Formula.SiblingNumber ); 
				}
			}
		}

		#endregion // TokenSortComparer Class

		#region ReferenceHolderSortNumberComparer

		internal class ReferenceHolderSortNumberComparer : IComparer
		{
			public int Compare( object xVal, object yVal )
			{
				ReferenceHolder x = xVal as ReferenceHolder;
				ReferenceHolder y = yVal as ReferenceHolder;

				int ix = null != x ? x.Formula.DependancySortNumber : -1;
				int iy = null != y ? y.Formula.DependancySortNumber : -1;

				return ix < iy ? -1 : ( ix > iy ? 1 : 0 );
			}
		}

		#endregion // ReferenceHolderSortNumberComparer 

		#region GetMaxSortNumber



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal static int GetMaxSortNumber( ReferenceHolder[] formulas, int startIndex, int endIndex )
		{
			int maxSortNumber = formulas[ startIndex ].Formula.DependancySortNumber;

			for ( int i = 1 + startIndex; i <= endIndex; i++ )
				maxSortNumber = Math.Max( maxSortNumber, formulas[ i ].Formula.DependancySortNumber );

			return maxSortNumber;
		}

		#endregion // GetMaxSortNumber

		#region HasScopeOther



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal static bool HasScopeOther( HashSet tokens )
		{
			foreach ( TokenInfo t in tokens )
			{
				if ( TokenInfo.ScopeType.Other == t.Scope )
					return true;
			}

			return false;
		}

		#endregion // HasScopeOther 

		#region SiblingFormulaSortComparer Class



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		internal class SiblingFormulaSortComparer : IComparer
		{

			private Dictionary<ReferenceHolder, int> sortPriorityTable = new Dictionary<ReferenceHolder, int>( );




			internal SiblingFormulaSortComparer()
			{
			}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

			private int GetSibilingSortPriority( ReferenceHolder formulaReference )
			{
				if ( this.sortPriorityTable.ContainsKey( formulaReference ) )
					return (int)this.sortPriorityTable[ formulaReference ];

				// MD 8/17/08 - Excel formula solving
				// Refactoring - Renamed UltraCalcFormula to UltraCalcFormulaBase
				//UltraCalcFormula formula = formulaReference.Formula;
				UltraCalcFormulaBase formula = formulaReference.Formula;

				int priority = formula.BaseReferenceHolder.refersBackToItself ? 2 : 1;

				// MD 7/30/08 - Excel formula solving
				// We shouldn't be using the formula tokens to find references. Not all references in the tokens are static references
				// so iterate all references actually used by the colletion.
				//foreach ( UltraCalcFormulaToken formulaToken in formula.Tokens )
				//{				
				//    UltraCalcFormula childFormula;
				//    IUltraCalcReference reference = TokenInfo.GetFormulaReferenceFromToken( formulaToken, out childFormula );
				foreach ( IUltraCalcReference reference in formula.AllReferences )
				{
					// The reference might be a UCReference. This was being unwrapped by the GetFormulaReferenceFromToken
					// which was claled before. It must now be unwrapped manually. All references to reference in the rest 
					// of the loop have been replaced by references to resolvedReference.
					// MD 8/26/08 - BR35804
					// We only want to unwrap UCReferences.
					//IUltraCalcReference resolvedReference = UltraCalcEngine.GetResolvedReference( reference );
					IUltraCalcReference resolvedReference = UltraCalcEngine.GetResolvedReference( reference, true );

					// MD 8/17/08 - Excel formula solving
					// Refactoring - Renamed UltraCalcFormula to UltraCalcFormulaBase
					//UltraCalcFormula childFormula = resolvedReference.Formula as UltraCalcFormula;
					UltraCalcFormulaBase childFormula = resolvedReference.Formula as UltraCalcFormulaBase;

					if ( null != childFormula )
					{
						bool isSibling = resolvedReference.IsSiblingReference( formula.BaseReference );

						if ( isSibling )
						{
							TokenInfo.ScopeType scope;
							int scopeIndex;
							TokenInfo.GetScopeInfo( resolvedReference, out scope, out scopeIndex );
							if ( ScopeType.Other == scope )
							{
								priority = 4;
								break;
							}
						}
						else 
						{
							priority = 0;
						}
					}
				}

				this.sortPriorityTable[ formulaReference ] = priority;
				return priority;
			}

			public int Compare( object objX, object objY )
			{
				ReferenceHolder x = objX as ReferenceHolder;
				ReferenceHolder y = objX as ReferenceHolder;

				int ix = null != x ? this.GetSibilingSortPriority( x ) : -1;
				int iy = null != y ? this.GetSibilingSortPriority( y ) : -1;

				return ix < iy ? -1 : ( ix > iy ? 1 : 0 );
			}
		}

		#endregion // SiblingFormulaSortComparer Class 

		#region SwapItems

		internal static void SwapItems( object[] array, int index1, int index2 )
		{
			object tmp = array[ index1 ];
			array[ index1 ] = array[ index2 ];
			array[ index2 ] = tmp;
		}

		#endregion // SwapItems 

		#region GetReferenceHolders



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal static HashSet GetReferenceHolders( HashSet tokens )
		{
			HashSet referenceHolders = new HashSet();

			foreach ( TokenInfo t in tokens )
				referenceHolders.Add( t.ReferenceHolder );

			return referenceHolders;
		}

		#endregion // GetReferenceHolders 

		#region Create0Token
			


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal static TokenInfo Create0Token( ReferenceHolder rh )
		{
			return new TokenInfo( rh
				, TokenInfo.ScopeType.RelativeIndex, 0  
				);
		}

		#endregion // Create0Token

		#region GetTokenWithMaxRelativeIndex



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static TokenInfo GetTokenWithMinRelativeIndex( HashSet tokens )
		{
			return TokenInfo.GetTokenHelper( tokens, true );
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static TokenInfo GetTokenWithMaxRelativeIndex( HashSet tokens )
		{
			return TokenInfo.GetTokenHelper( tokens, false );
		}

		private static TokenInfo GetTokenHelper( HashSet tokens, bool min )
		{
			TokenInfo token = null;

			foreach ( TokenInfo t in tokens )
			{
				if ( null == token || TokenInfo.ScopeType.RelativeIndex != t.Scope
					|| ( min ? t.ScopeIndex < token.ScopeIndex : t.ScopeIndex > token.ScopeIndex ) )
				{
					token = t;

					// A token with scope of Other (like A(*)) has priority over any token with relative index.
					//
					if ( TokenInfo.ScopeType.RelativeIndex != t.Scope )
						break;
				}
			}

			return token;
		}

		#endregion // GetTokenWithMaxRelativeIndex 
		#region IsDependant



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		// SSP 6/27/07 - BR24314 - Optimizations
		// Added cache parameter.
		// 
		//internal static bool IsDependant( TokenInfo tokenLHS, TokenInfo tokenRHS )
		internal static bool IsDependant( AncestorMapCache cache, TokenInfo tokenLHS, TokenInfo tokenRHS )
		{
			// SSP 6/27/07 - BR24314 - Optimizations
			// Added cache parameter.
			// 
			//HashSet dependants = TokenInfo.GetDependants( tokenLHS, tokenRHS.ReferenceHolder );
			HashSet dependants = TokenInfo.GetDependants( cache, tokenLHS, tokenRHS.ReferenceHolder );

			foreach ( TokenInfo t in dependants )
			{
				if ( tokenRHS.IntersectsWith( t ) )
					return true;
			}

			return false;
		}

		#endregion // IsDependant

		// MD 7/30/08 - Excel formula solving
		// This method has been removed because it is no longer used
		#region Not Used

		
#region Infragistics Source Cleanup (Region)

































#endregion // Infragistics Source Cleanup (Region)



		#endregion Not Used

		#region GetDependants



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		// SSP 6/27/07 - BR24314 - Optimizations
		// Added cache parameter.
		// 
		//internal static HashSet GetDependants( ReferenceHolder LHS, ReferenceHolder RHS )
		internal static HashSet GetDependants( AncestorMapCache cache, ReferenceHolder LHS, ReferenceHolder RHS )
		{
			// SSP 6/27/07 - BR24314 - Optimizations
			// Added cache parameter.
			// 
			//return TokenInfo.GetDependants( TokenInfo.Create0Token( LHS ), RHS );
			return TokenInfo.GetDependants( cache, TokenInfo.Create0Token( LHS ), RHS );
		}
		


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		// SSP 6/27/07 - BR24314 - Optimizations
		// Added cache parameter.
		// 
		//internal static HashSet GetDependants( TokenInfo tokenLHS, ReferenceHolder RHS )
		internal static HashSet GetDependants( AncestorMapCache cache, TokenInfo tokenLHS, ReferenceHolder RHS )
		{
			// SSP 6/27/07 - BR24314 - Optimizations
			// 
			// --------------------------------------------------------------------
			// MD 12/1/10 - TFS58742
			// Refactored this becasue the caching logic is now moved to the GetDependantsHelper method. This is so we 
			// can generate caches for additional tokens as we go through nested calls. This way, when we need to get 
			// dependants for a token and we already walked over it from a nested call previously, we can use the cached 
			// result.
			//HashSet result = null;
			//
			//if (cache.tokensCache.ContainsKey(tokenLHS))
			//{
			//    result = cache.tokensCache[tokenLHS] as HashSet;
			//} 
			//
			//if ( null == result )
			//{
			//    result = new HashSet( );
			//    HashSet processedLHS = new HashSet( );
			//    TokenInfo.GetDependantsHelper( tokenLHS, null, processedLHS, result );
			//
			//    cache.tokensCache.Add( tokenLHS, result );
			//}
			HashSet processedLHS = new HashSet();
			HashSet result = TokenInfo.GetDependantsHelper(cache, tokenLHS, processedLHS);

			if ( null != RHS )
				return TokenInfo.FilterDependants( result, RHS );

			return result;
			
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

			// --------------------------------------------------------------------
		}



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		// MD 12/1/10 - TFS58742
		// The RHS value passed inhere is always null, so I am removing it and changing the code in this method that uses it.
		// Also, add an AncestorMapCache parameter because we will now populate and use it in this method.
		// Also, the method wil lnow return the HashSet instead of taking it in, because it now contains the caching logic.
		//private static void GetDependantsHelper( TokenInfo tokenLHS, ReferenceHolder RHS, HashSet processedLHS, HashSet result )
		private static HashSet GetDependantsHelper(AncestorMapCache cache, TokenInfo tokenLHS, HashSet processedLHS)
		{
			// Don't expand the same formula more than once in the same path down to a leaf in
			// the virtual tree that the formula and dependant tokens form.
			//
			if ( null != tokenLHS.Formula && ! processedLHS.Exists( tokenLHS.ReferenceHolder ) )
			{
				// MD 12/1/10 - TFS58742
				// Check the cache first. If the token already had its dependents cached, return them.
				HashSet result;
				if (cache.tokensCache.TryGetValue(tokenLHS, out result))
					return result;

				// MD 12/1/10 - TFS58742
				// Otherwise, create a new cache state and initialize the value indicating whether we should cache the token's 
				// dependents. We will always cache unless this token is part of a circular reference and it is not the first token
				// in the cirular chain for which we are getting dependents.
				result = new HashSet();
				bool shouldStoreCache = true;

				processedLHS.Add( tokenLHS.ReferenceHolder );

				try
				{
					// MD 12/1/10 - TFS58742
					// If this formula references the same token twice, such as =A1+A1, we don't want to walk into the reference 
					// twice, so we need ot keep track of which tokens on which we made nested calls to GetDependantsHelper.
					HashSet visitedTokens = null;

					// MD 7/30/08 - Excel formula solving
					// We shouldn't be using the formula tokens to find references. Not all references in the tokens are static references
					// so iterate all references actually used by the colletion.
					//foreach ( UltraCalcFormulaToken formulaToken in tokenLHS.Formula.Tokens )
					//{
					//    // We are only concerned with tokens that have formulas. Also if RHS was specified
					//    // then check to make sure the reference matches the RHS.
					//    //
					//    UltraCalcFormula formula;
					//    IUltraCalcReference referenceToken = TokenInfo.GetFormulaReferenceFromToken( formulaToken, out formula );
					foreach ( IUltraCalcReference referenceToken in tokenLHS.Formula.AllReferences )
					{
						// The referenceToken might be a UCReference. This was being unwrapped by the GetFormulaReferenceFromToken
						// which was claled before. It must now be unwrapped manually. All references to referenceToken in the rest 
						// of the loop have been replaced by references to referenceTokenResolved.
						// MD 8/26/08 - BR35804
						// We only want to unwrap UCReferences.
						//IUltraCalcReference referenceTokenResolved = UltraCalcEngine.GetResolvedReference( referenceToken );
						IUltraCalcReference referenceTokenResolved = UltraCalcEngine.GetResolvedReference( referenceToken, true );

						// SSP 9/4/08 BR31006
						// 
						//if ( referenceTokenResolved.Formula == null )
						UltraCalcFormulaBase formula = referenceTokenResolved.Formula as UltraCalcFormulaBase;
						// SSP 10/7/11
						// If the BaseReferenceHolder is null then that means the formula was not added to the ancestor
						// map. This can be the case when the formula has a syntax error. In which case we don't add
						// the formula to the ancestor map. We should skip such formulas.
						// 
						//if ( formula == null )
						if ( formula == null || null == formula.BaseReferenceHolder )
							continue;

						if ( null != referenceTokenResolved )
						{
							// SSP 9/4/08 BR31006
							// Added referenceHolder parameter. Use that instead of creating a new one from reference.
							//
							//TokenInfo newToken = new TokenInfo( referenceTokenResolved, tokenLHS );
							TokenInfo newToken = new TokenInfo( formula.BaseReferenceHolder, referenceTokenResolved, tokenLHS );

							// MD 12/1/10 - TFS58742
							// If this formula references the same token twice, such as =A1+A1, we don't want to walk into the reference 
							// twice, so make sure it has not already been visited. If it has, move to the next reference.
							if (visitedTokens == null)
								visitedTokens = new HashSet();

							if (visitedTokens.Exists(newToken))
								continue;

							visitedTokens.Add(newToken);

						    // SSP 6/25/07 - BR24314 - Optimizations
						    // Don't bother calling IsSameBaseReference if the token's already in the result.
						    // 
							// MD 12/1/10 - TFS58742
							// RHS was always null in this method, so we always added the token to the HashSet. RHS is no longer passed in
							// so we will just add the token always.
						    //if ( null == RHS || ReferenceHolder.IsSameBaseReference( RHS.Reference, referenceToken ) )
							//if ( null == RHS || !result.Exists( newToken ) && ReferenceHolder.IsSameBaseReference( RHS.Reference, referenceTokenResolved ) )
							//	result.Add( newToken );
							result.Add(newToken);

							// MD 12/1/10 - TFS58742
							// This method now returns a HashSet containing the token's dependents, so get it and merge it into the overall 
							// result HashSet.
						    //TokenInfo.GetDependantsHelper( newToken, RHS, processedLHS, result );	
							HashSet nestedResult = TokenInfo.GetDependantsHelper(cache, newToken, processedLHS);

							// If we are in the middle of a circular refernce chain and up the call stack is a call to GetDependantsHelper 
							// for another reference in the chain, the result from GetDependantsHelper will be null when calling it on the 
							// same reference. If that is the case, it's dependants collection is not yet complete, so skip it and also, 
							// don't store this reference's dependants in the cache, becasue it needs to have the circular reference's 
							// dependants completed. So set shouldStoreCache to False and finish finding all references. When we get 
							// dependants on this reference later, we will then cache it's dependants.
							if (nestedResult == null)
							{
								shouldStoreCache = false;
								continue;
							}

							result.AddItems(nestedResult);						
						}
					}

					// MD 12/1/10 - TFS58742
					return result;
				}
				finally
				{
					// MD 12/1/10 - TFS58742
					// Add an entry to the AncestorMapCache for the token.
					if (shouldStoreCache)
						cache.tokensCache.Add(tokenLHS, result);

					processedLHS.Remove( tokenLHS.ReferenceHolder );
				}
			}

			// MD 12/1/10 - TFS58742
			return null;
		}

		#endregion // GetDependants

		#region FilterDependants

		// SSP 6/26/07 - BR24314 - Optimizations
		// Added FilterDependants method.
		// 
		internal static HashSet FilterDependants( HashSet tokens, ReferenceHolder RHS )
		{
			HashSet ret = new HashSet( );

			foreach ( TokenInfo tt in tokens )
			{
				if ( null == RHS || ReferenceHolder.IsSameBaseReference( RHS.Reference, tt.Reference ) )
					ret.Add( tt );
			}

			return ret;
		}

		// SSP 9/5/07 BR25911
		// Added FilterDependants that takes in multiple RHS.
		// 
		internal static HashSet FilterDependants( HashSet tokens, IEnumerable rhsReferenceHolders )
		{
			HashSet ret = new HashSet();

			foreach ( TokenInfo tt in tokens )
			{
				foreach ( ReferenceHolder rhs in rhsReferenceHolders )
				{
					if ( ReferenceHolder.IsSameBaseReference( rhs.Reference, tt.Reference ) )
						ret.Add( tt );
				}
			}

			return ret;
		} 

		#endregion // FilterDependants

		#region SetRecalcDeferredToFalse



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static void SetRecalcDeferredToFalse( ReferenceHolder rh )
		{
			Debug.Assert( null != rh && null != rh.Formula );
			if ( null != rh && rh.Reference.RecalcDeferred && null != rh.Formula )
			{
				rh.Reference.RecalcDeferred = false;

				// MD 7/30/08 - Excel formula solving
				// We shouldn't be using the formula tokens to find references. Not all references in the tokens are static references
				// so iterate all references actually used by the colletion.

				//foreach ( UltraCalcFormulaToken formulaToken in rh.Formula.Tokens )
				//{
				//    UltraCalcFormula childFormula;
				//    IUltraCalcReference childReference = TokenInfo.GetFormulaReferenceFromToken( formulaToken, out childFormula );
				foreach ( IUltraCalcReference childReference in rh.Formula.AllReferences )
				{
					// The childReference might be a UCReference. This was being unwrapped by the GetFormulaReferenceFromToken
					// which was claled before. It must now be unwrapped manually. All references to childReference in the rest 
					// of the loop have been replaced by references to childReferenceResolved.
					// MD 8/26/08 - BR35804
					// We only want to unwrap UCReferences.
					//IUltraCalcReference childResolvedReference = UltraCalcEngine.GetResolvedReference( childReference );
					IUltraCalcReference childResolvedReference = UltraCalcEngine.GetResolvedReference( childReference, true );

					// MD 8/17/08 - Excel formula solving
					// Refactoring - Renamed UltraCalcFormula to UltraCalcFormulaBase
					//UltraCalcFormula childFormula = childResolvedReference.Formula as UltraCalcFormula;
					UltraCalcFormulaBase childFormula = childResolvedReference.Formula as UltraCalcFormulaBase;

					// SSP 10/7/11
					// If the BaseReferenceHolder is null then that means the formula was not added to the ancestor
					// map. This can be the case when the formula has a syntax error. In which case we don't add
					// the formula to the ancestor map. We should skip such formulas.
					// 
					//if ( null != childFormula )
					if ( null != childFormula && null != childFormula.BaseReferenceHolder )
						TokenInfo.SetRecalcDeferredToFalse( childFormula.BaseReferenceHolder );
				}
			}
		}

		#endregion // SetRecalcDeferredToFalse
	}

	#endregion // TokenInfo Class

	#region ReferenceHolder Class



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

	internal class ReferenceHolder
	{
		#region Private Vars

		// MD 8/27/08 - Code Analysis - Performance
		//private IUltraCalcReference reference = null;
		private IUltraCalcReference reference;

		// MD 8/17/08 - Excel formula solving
		// Refactoring - Renamed UltraCalcFormula to UltraCalcFormulaBase
		//private UltraCalcFormula    referenceFormula = null;
		// MD 8/27/08 - Code Analysis - Performance
		//private UltraCalcFormulaBase referenceFormula = null;
		private UltraCalcFormulaBase referenceFormula;

		// DetectCircularity sets this flag to true if the formula refers to itself. This
		// doen't necessarily mean the formula has a logical circularity.
		//
		// MD 8/27/08 - Code Analysis - Performance
		//internal bool refersBackToItself = false;
		internal bool refersBackToItself;

		// SSP 12/16/05 BR01209
		// Added origRecalcDeferred flag.
		//
		// MD 8/27/08 - Code Analysis - Performance
		//internal bool origRecalcDeferred = false;
		internal bool origRecalcDeferred = false; 

		#endregion // Private Vars

		#region Constructor



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		// MD 8/17/08 - Excel formula solving
		// Refactoring - Renamed UltraCalcFormula to UltraCalcFormulaBase
		//internal ReferenceHolder( IUltraCalcReference reference, UltraCalcFormula referenceFormula )
		internal ReferenceHolder( IUltraCalcReference reference, UltraCalcFormulaBase referenceFormula )
		{
			if ( null == reference )
				throw new ArgumentNullException( "reference" );

			this.reference = reference;

			// MD 8/17/08 - Excel formula solving
			// Refactoring - Renamed UltraCalcFormula to UltraCalcFormulaBase
			//this.referenceFormula = null != referenceFormula ? referenceFormula : (UltraCalcFormula)reference.Formula;
			this.referenceFormula = null != referenceFormula ? referenceFormula : (UltraCalcFormulaBase)reference.Formula;
		}

		#endregion // Constructor

		#region Reference






		internal IUltraCalcReference Reference
		{
			get
			{
				return this.reference;
			}
		}

		#endregion // Reference

		#region Formula






		// MD 8/17/08 - Excel formula solving
		// Refactoring - Renamed UltraCalcFormula to UltraCalcFormulaBase
		//internal UltraCalcFormula Formula
		internal UltraCalcFormulaBase Formula
		{
			get
			{
				return this.referenceFormula;
			}
		}			

		#endregion // Formula

		#region GetHashCode

		public override int GetHashCode( )
		{
			return this.reference.GetHashCode( );
		}

		#endregion // GetHashCode

		#region Equals

		public override bool Equals( object obj )
		{
			ReferenceHolder rh = obj as ReferenceHolder;
			return null != rh && UltraCalcEngine.CompareReferences( this.reference, rh.reference );
		}

		#endregion // Equals

		#region IsSameBaseReference

		internal static bool IsSameBaseReference( IUltraCalcReference reference1, IUltraCalcReference reference2 )
		{
			return reference1.ContainsReference( reference2 );
		}

		#endregion // IsSameBaseReference
	}

	#endregion // ReferenceHolder Class

	#region HashSet






	internal class HashSet : ICollection
	{
		#region Private Vars

		private static readonly object DUMMY_VALUE = DBNull.Value;

		// MD 8/27/08 - Code Analysis - Performance
		//private Hashtable table = null;
        private Dictionary<object, object> table;

		#endregion // Private Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		public HashSet( )
		{
		    this.table = new Dictionary<object, object>();
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public HashSet( int initialCapacity )
		{
            this.table = new Dictionary<object, object>(initialCapacity);
		}

        // MD 11/25/09 - Code sharing with Silverlight
        ///// <summary>
        ///// Constructor.
        ///// </summary>
        //public HashSet( int initialCapacity, float loadFactor )
        //{
        //    this.table = new Hashtable( initialCapacity, loadFactor );
        //} 

		#endregion // Constructor

		#region Add

		/// <summary>
		/// Adds the item to the set. If the item already exists in the set, does nothing.
		/// </summary>
		/// <param name="item"></param>
		public void Add( object item )
		{
			this.table[ item ] = DUMMY_VALUE;
		}

		#endregion // Add

		#region Remove

		/// <summary>
		/// Removes the specified item from the set. If the item doesn't exist in the set
		/// does nothing.
		/// </summary>
		/// <param name="item"></param>
		public void Remove( object item )
		{
			this.table.Remove( item );
		}

		#endregion // Remove

		#region Exists

		/// <summary>
		/// Returns true if the specified item exists in this set.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Exists( object item )
		{
			return this.table.ContainsKey( item );
		}

		#endregion // Exists

		#region AddItems

		/// <summary>
		/// Adds items from the specified set to this set.
		/// </summary>
		/// <param name="source"></param>
		public void AddItems( HashSet source )
		{
			foreach ( object item in source )
				this.Add( item );
		}

		#endregion // AddItems

		#region GetUnion

		/// <summary>
		/// Calculates the union of the specified sets.
		/// </summary>
		/// <param name="set1"></param>
		/// <param name="set2"></param>
		/// <returns></returns>
		public static HashSet GetUnion( HashSet set1, HashSet set2 )
		{
			HashSet result = new HashSet( );
			result.AddItems( set1 );
			result.AddItems( set2 );

			return result;
		}

		#endregion // GetUnion

		#region GetIntersection

		/// <summary>
		/// Calculates the intersection of the specified sets.
		/// </summary>
		/// <param name="set1"></param>
		/// <param name="set2"></param>
		/// <returns></returns>
		public static HashSet GetIntersection( HashSet set1, HashSet set2 )
		{
			HashSet result = new HashSet( );

			if ( set1.Count > set2.Count )
				return GetIntersection( set2, set1 );

			foreach ( object item in set1 )
			{
				if ( set2.Exists( item ) )
					result.Add( item );
			}

			return result;
		}

		#endregion // GetIntersection

		#region DoesIntersect

		/// <summary>
		/// Returns true of the specified set and this set intersect.
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public bool DoesIntersect( HashSet s )
		{
			if ( this.Count > s.Count )
				return s.DoesIntersect( this );

			foreach ( object item in this )
			{
				if ( s.Exists( item ) )
					return true;
			}

			return false;
		}

		#endregion // DoesIntersect

		#region IsSubsetOf

		/// <summary>
		/// Returns true if this set is a subset of the specified set.
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public bool IsSubsetOf( HashSet s )
		{
			if ( this.Count > s.Count )
				return false;

			foreach ( object item in this )
			{
				if ( ! s.Exists( item ) )
					return false;
			}

			return true;
		}

		#endregion // IsSubsetOf

		#region Clear

		/// <summary>
		/// Clears the set.
		/// </summary>
		public void Clear( )
		{
			this.table.Clear( );
		}

		#endregion // Clear

		#region Count

		/// <summary>
		/// Returns the number of items contained in the set.
		/// </summary>
		public int Count
		{
			get
			{
				return this.table.Count;
			}
		}

		#endregion // Count

		#region IsEmpty

		/// <summary>
		/// Returns true if the set is empty, that is it has no elements.
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				return 0 == this.Count;
			}
		}

		#endregion // IsEmpty

		#region GetEnumerator

		/// <summary>
		/// Returns a new enumerator that enumerates all the elements of this set.
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator( )
		{
			return this.table.Keys.GetEnumerator( );
		}

		#endregion // GetEnumerator

		#region IsSynchronized

		/// <summary>
		/// Indicates whether this data structure is synchronized.
		/// </summary>
		public bool IsSynchronized
		{
			get
			{
			    return CalcManagerUtilities.IsSynchronized(this.table);
			}
		}

		#endregion // IsSynchronized

		#region CopyTo

		/// <summary>
		/// Copies all the elements of this set to the spcified array starting at the specified index in the array.
		/// </summary>
		/// <param name="array"></param>
		/// <param name="arrayIndex"></param>
		public void CopyTo( System.Array array, int arrayIndex )
		{
			this.table.Keys.CopyTo( array as object[], arrayIndex );
		}

		#endregion // CopyTo

		#region SyncRoot

		/// <summary>
		/// Returns the object that can be used to synchronize the access to this data structure.
		/// </summary>
		public object SyncRoot
		{
			get
			{
                return CalcManagerUtilities.SyncRoot(this.table);
			}
		}

		#endregion // SyncRoot

		#region ToArray

		/// <summary>
		/// Returns an array containing all the elements of this set.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public object[] ToArray( Type type )
		{
			Array arr = Array.CreateInstance( type, this.Count );
			this.CopyTo( arr, 0 );
			return (object[])arr;
		}

		#endregion // ToArray
	}

	#endregion // HashSet

	#region RangeCalcInfo






	internal class RangeCalcInfo 
	{
		#region Private Vars

		private object[] backTrackRows = null;
		private int backTrackRowCount = 0;
		private int index = 0;

		private IEnumerator referenceEnumerator = null;
		private int referenceEnumeratorExhaustedCounter = 0;
		internal IUltraCalcReference[] referencesBeingEvaluated = null;

		// MD 8/17/08 - Excel formula solving
		// Refactoring - Renamed UltraCalcFormula to UltraCalcFormulaBase
		//private UltraCalcFormula[] formulasBeingEvaluated = null;
		private UltraCalcFormulaBase[] formulasBeingEvaluated = null;

		private UCRecalcChain recalcChain = null;
		private UltraCalcEngine calcEngine = null;
		private int recalcChainVersion = -1;
		private int ancestorMapVersion = -1;

		#endregion // Private Vars

		#region Constructor

		private RangeCalcInfo( )
		{
		}

		#endregion // Constructor

		#region CreateNew
		


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal static RangeCalcInfo CreateNew( UCRecalcChain recalcChain )
		{
			RangeCalcInfo rangeCalcInfo = new RangeCalcInfo( );
			if ( rangeCalcInfo.Initialize( recalcChain ) )
				return rangeCalcInfo;

			return null;
		}

		#endregion // CreateNew

		#region Initialize



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		private bool Initialize( UCRecalcChain recalcChain )
		{
			this.recalcChain = recalcChain;
			this.calcEngine = recalcChain.CalcEngine;
			IUltraCalcReference reference = this.recalcChain[0];

			// MD 8/17/08 - Excel formula solving
			// Refactoring - Renamed UltraCalcFormula to UltraCalcFormulaBase
			//UltraCalcFormula formula = (UltraCalcFormula)reference.Formula;
			UltraCalcFormulaBase formula = (UltraCalcFormulaBase)reference.Formula;

			Debug.Assert( null != formula, string.Format( "Reference {0} without formula encountered on the recalc chain !", reference.AbsoluteName ) );
			if ( null == formula )
				return false;

			// Only enumerable references can be range recalced.
			//
			Debug.Assert( reference.IsEnumerable );
			if ( ! reference.IsEnumerable )
				return false;

			this.referenceEnumerator = reference.References.GetEnumerator( );

			// If the formula requires that the cells be calculated from bottom to top (like in A=A(+1))
			// then get the reverse iterator.
			//
			if ( RowIterationType.Backward == formula.RowIterationTypeResolved )
				this.referenceEnumerator = this.GetReverseEnumerator( this.referenceEnumerator );

			int formulaCount = 1;
			int maxAbsoluteRowOffset = Math.Abs( formula.EvaluationRowOffset );
			while ( formulaCount < this.recalcChain.Count )
			{
				IUltraCalcReference currRef = this.recalcChain[ formulaCount ];
				if ( ! currRef.IsSiblingReference( reference ) )
					break;

				// MD 8/17/08 - Excel formula solving
				// Refactoring - Renamed UltraCalcFormula to UltraCalcFormulaBase
				//UltraCalcFormula tmpFormula = (UltraCalcFormula)currRef.Formula;
				UltraCalcFormulaBase tmpFormula = (UltraCalcFormulaBase)currRef.Formula;

				Debug.Assert( null != tmpFormula, string.Format( "Reference {0} without formula encountered on the recalc chain !", currRef.AbsoluteName ) );
				if ( null != tmpFormula && ( tmpFormula.EvaluationGroupNumber < 0 || formula.EvaluationGroupNumber != tmpFormula.EvaluationGroupNumber ) )
					break;

				formulaCount++;
				maxAbsoluteRowOffset = Math.Max( maxAbsoluteRowOffset, Math.Abs( tmpFormula.EvaluationRowOffset ) );
			}

			// Copy the references into the formulasBeingEvaluated array.
			//
			this.referencesBeingEvaluated = new IUltraCalcReference[ formulaCount ];

			// MD 8/17/08 - Excel formula solving
			// Refactoring - Renamed UltraCalcFormula to UltraCalcFormulaBase
			//this.formulasBeingEvaluated = new UltraCalcFormula[ formulaCount ];
			this.formulasBeingEvaluated = new UltraCalcFormulaBase[ formulaCount ];

			for ( int i = 0; i < formulaCount; i++ )
			{
				this.referencesBeingEvaluated[ i ] = this.recalcChain[ i ];

				// MD 8/17/08 - Excel formula solving
				// Refactoring - Renamed UltraCalcFormula to UltraCalcFormulaBase
				//this.formulasBeingEvaluated[ i ] = (UltraCalcFormula)this.referencesBeingEvaluated[ i ].Formula;
				this.formulasBeingEvaluated[ i ] = (UltraCalcFormulaBase)this.referencesBeingEvaluated[ i ].Formula;
			}

			// Remove the references from the recalc chain.
			//
			this.recalcChain.RemoveRange( 0, formulaCount );

			// The formula group will require us to backtrack the one plus the difference between
			// the smallest and the largest relative index in formula tokens.
			//
			this.backTrackRowCount = 1 + maxAbsoluteRowOffset;
			this.backTrackRows = new object[ this.backTrackRowCount ];
			this.index = this.backTrackRowCount;
			this.recalcChainVersion = this.recalcChain.RecalcChainVersion;
			this.ancestorMapVersion = this.CalcEngine.AncestorMapVersion;

			return true;
		}

		#endregion // Initialize

		#region CalcEngine

		internal UltraCalcEngine CalcEngine
		{
			get
			{
				return this.calcEngine;
			}
		}

		#endregion // CalcEngine

		#region EvaluateNextRow



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal bool EvaluateNextRow( )
		{
			object nextRow = null;
			if ( 0 == this.referenceEnumeratorExhaustedCounter && this.referenceEnumerator.MoveNext( ) )
				nextRow = this.referenceEnumerator.Current;
			else
				this.referenceEnumeratorExhaustedCounter++;

			if ( this.referenceEnumeratorExhaustedCounter >= this.backTrackRowCount )
				return false;

			this.Add( nextRow );

			for ( int i = 0; i < this.referencesBeingEvaluated.Length; i++ )
			{
				IUltraCalcReference columnRef = this.referencesBeingEvaluated[i];

				// MD 8/17/08 - Excel formula solving
				// Refactoring - Renamed UltraCalcFormula to UltraCalcFormulaBase
				//UltraCalcFormula formula = this.formulasBeingEvaluated[i];
				UltraCalcFormulaBase formula = this.formulasBeingEvaluated[ i ];

				// If something changes like items get dirtied or a column gets deleted etc. then don't
				// continue with evaluation.
				//
				if ( this.ChangeDetected )
					return false;

				if ( null != formula )
				{
					IUltraCalcReference cellRef = this[ formula.EvaluationRowOffset ];
					if ( null != cellRef )
					{
						// Now get the cell belonging to the current column. For the first formula we don't
						// need to call resolve because the enumerator will be from the first formula so the 
						// cell reference will belong to the right column.
						//
						if ( 0 != i )
							cellRef = cellRef.ResolveReference( columnRef, ResolveReferenceType.RightHandSide );
						Debug.Assert( null != cellRef, "No cell in the corresponding column !" );
						if ( null != cellRef )
						{
							cellRef.Value = formula.Evaluate( cellRef );

							// Raise an event that its value has been cleaned
							this.CalcEngine.RaiseRecalcValueEvent( cellRef );

							// Denote we've calculated the visible cell
							if ( this.CalcEngine.EnableMarkedColumns )
								cellRef.RecalcVisible = false;
						}
					}
				}
			}

			return true;
		}

		#endregion // EvaluateNextRow

		#region GetReverseEnumerator

		private IEnumerator GetReverseEnumerator( IEnumerator enumerator )
		{
			ArrayList list = new ArrayList( );
			
			enumerator.Reset( );
			while ( enumerator.MoveNext( ) )
				list.Add( enumerator.Current );

			list.Reverse( );
			return list.GetEnumerator( );
		}

		#endregion // GetReverseEnumerator

		#region ChangeDetected

		internal bool ChangeDetected
		{
			get
			{
				if ( this.ancestorMapVersion != this.CalcEngine.AncestorMapVersion 
					|| this.CalcEngine.EventQueue.Count > 0 )
					return true;

				if ( this.recalcChainVersion != this.recalcChain.RecalcChainVersion )
				{
					// Stop evaluating any items that got listed for recalc.
					//
					int remainingReferences = this.referencesBeingEvaluated.Length;
					for ( int i = 0; i < this.referencesBeingEvaluated.Length; i++ )
					{
						if ( this.recalcChain.IsRecalcListed( this.referencesBeingEvaluated[i] ) )
						{
							// Null out the item that got listed.
							//
							this.referencesBeingEvaluated[i] = null;
							this.formulasBeingEvaluated[i] = null;
							remainingReferences--;
						}
					}

					// If all the formulas that we are evaluating got relisted on the recalc chain
					// then we have to stop evaluating at this point.
					//
					if ( 0 == remainingReferences )
						return true;

					this.recalcChainVersion = this.recalcChain.RecalcChainVersion;
				}

				return false;
			}
		}

		#endregion // ChangeDetected

		#region Add

		internal void Add( object item )
		{
			this.backTrackRows[ ++this.index % this.backTrackRowCount ] = item;
		}

		#endregion // Add

		#region Indexer

		internal IUltraCalcReference this[ int negativeIndex ]
		{
			get
			{
				return (IUltraCalcReference)this.backTrackRows[ ( this.index + negativeIndex ) % this.backTrackRowCount ];
			}
		}

		#endregion // Indexer

		#region IsBeingEvaluated
		


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal bool IsBeingEvaluated( IUltraCalcReference reference )
		{
			for ( int i = 0; i < this.referencesBeingEvaluated.Length; i++ )
			{
				if ( UltraCalcEngine.CompareReferences( reference, this.referencesBeingEvaluated[i] ) )
					return true;
			}

			return false;
		}

		#endregion // IsBeingEvaluated

		#region CancelRangeRecalc
		





		internal void CancelRangeRecalc( )
		{
			for ( int i = 0; i < this.referencesBeingEvaluated.Length; i++ )
			{
				IUltraCalcReference reference = this.referencesBeingEvaluated[i];
				if ( null != reference && null != reference.Formula )
					this.recalcChain.Add( reference );
			}
		}

		#endregion // CancelRangeRecalc

		#region Dump

		internal void Dump( )
		{
			foreach ( IUltraCalcReference reference in this.formulasBeingEvaluated )
				Debug.WriteLine( null != reference ? reference.AbsoluteName : "null " );
		}

		#endregion // Dump
	}

	#endregion // RangeCalcInfo

	#region AncestorMapCache Class

	// SSP 6/27/07 - BR24314 - Optimizations
	// Added AncestorMapCache.
	// 
	internal class AncestorMapCache
	{
		// MD 12/1/10 - TFS58742
		// There's no reason for this to just use objects. Changed this to use the types that will actually be used.
        //internal Dictionary<object, object> tokensCache = new Dictionary<object, object>();
		internal Dictionary<TokenInfo, HashSet> tokensCache = new Dictionary<TokenInfo, HashSet>();

		internal AncestorMapCache( UltraCalcEngine calcEngine )
		{
		}
	}

	#endregion // AncestorMapCache Class

	#region CalculateFormulaReference



#region Infragistics Source Cleanup (Region)















































































































































































































#endregion // Infragistics Source Cleanup (Region)


	#endregion // CalculateFormulaReference

	#region SubListEnumerable Class

	// SSP 9/5/07 BR25911 
	// Added SubListEnumerable for traversing a portion of a list.
	// 
	internal class SubListEnumerable : IEnumerable
	{
		private Enumerator ee;

		public SubListEnumerable( IList list, int startIndex, int endIndex )
		{
			this.ee = new Enumerator( list, startIndex, endIndex );
		}

		public class Enumerator : IEnumerator
		{
			private IList list;
			private int startIndex;
			private int endIndex;
			private int ii;

			public Enumerator( IList list, int startIndex, int endIndex )
			{
				this.list = list;
				this.startIndex = startIndex;
				this.endIndex = endIndex;

				this.Reset( );
			}

			public object Current
			{
				get 
				{ 
					return this.list[ this.ii ]; 
				}
			}

			public bool MoveNext( )
			{
				this.ii++;

				return this.ii <= this.endIndex;
			}

			public void Reset( )
			{
				this.ii = this.startIndex - 1;
			}

			public Enumerator Clone( )
			{
				return (Enumerator)this.MemberwiseClone( );
			}
		}

		public IEnumerator GetEnumerator( )
		{
			return this.ee.Clone( );
		}

		public static IEnumerable SubElements( IList list, int startIndex, int endIndex )
		{
			return new SubListEnumerable( list, startIndex, endIndex );
		}
	}

	#endregion // SubListEnumerable Class

	internal class CalcManagerUtilities
    {
        internal static DateTime DateAndTimeDateAdd(string interval, double number, DateTime dateValue)
        {

            return SilverlightFixes.DateAndTimeDateAdd(SilverlightFixes.DateIntervalFromString(interval), number, dateValue);



        }

        internal static DateTime DateAndTimeDateAdd(DateInterval interval, double number, DateTime dateValue)
		{

			return SilverlightFixes.DateAndTimeDateAdd(interval, number, dateValue);



        }

        internal static long DateAndTimeDateDiff(string interval, DateTime date1, DateTime date2, FirstDayOfWeek firstDayOfWeek, FirstWeekOfYear firstWeekOfYear)
		{

			return SilverlightFixes.DateAndTimeDateDiff(SilverlightFixes.DateIntervalFromString(interval), date1, date2, firstDayOfWeek, firstWeekOfYear);



        }

        internal static long DateAndTimeDateDiff(DateInterval interval, DateTime date1, DateTime date2, FirstDayOfWeek firstDayOfWeek, FirstWeekOfYear firstWeekOfYear)
		{

			return SilverlightFixes.DateAndTimeDateDiff(interval, date1, date2, firstDayOfWeek, firstWeekOfYear);



        }

        [Conditional("DEBUG")]
        internal static void DebugFail(string message)
		{

			Debug.WriteLine("FAIL: " + message);



        }

        [Conditional("DEBUG")]
        internal static void DebugWriteLineIf(bool condition, string message)
		{

			if (condition)
            {
                Debug.WriteLine("FAIL: " + message);
            }



        }

        /// <summary>
        /// Get the values from enumeration
        /// </summary>
        /// <param name="enumType">Type of the enum.</param>
        /// <returns>array of values in the enumeration</returns>
        internal static System.Array EnumGetValues(Type enumType)
		{

			if (!enumType.IsEnum)
            {
                throw new ArgumentException(ExcelResources.GetString("LE_ArgumentException_NotEnum", enumType.Name));
            }

			// MD 11/1/11
			// Found while fixing TFS94534
			// We should return an array of the enum type, not an object array.
            //List<object> values = new List<object>();
			IList values = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(enumType));

            var fields = enumType.GetFields().Where(field => field.IsLiteral);

            foreach (FieldInfo field in fields)
            {
                object value = field.GetValue(enumType);
                values.Add(value);
            }

			// MD 11/1/11
			// Found while fixing TFS94534
			// We should return an array of the enum type, not an object array.
			//return values.ToArray();
			Array array = Array.CreateInstance(enumType, values.Count);
			values.CopyTo(array, 0);
			return array;



        }

        internal static bool IsSynchronized(ICollection syncObject)
		{

			return true;



        }

		// MD 3/18/12 - TFS105148
		// Moved to MathUtilities so it could be shared by both the shared and calc manager assemblies.
		#region Moved

		//        internal static double MathTruncate(double value)
		//        {
		//#if SILVERLIGHT
		//            return SilverlightFixes.MathTruncate(value);
		//#else
		//            return Math.Truncate(value);
		//#endif
		//        }

		#endregion // Moved

        internal static object SyncRoot(ICollection syncObject)
		{

			return syncObject;



        }

		// MD 3/18/12 - TFS105148
		// Moved to MathUtilities so it could be shared by both the shared and calc manager assemblies.
		#region Moved

		//// MD 6/7/11 - TFS78166
		//#region RoundToExcelDisplayValue

		//public static double RoundToExcelDisplayValue(double value)
		//{
		//    int roundingDigits = 14 - (int)Math.Floor(Math.Log10(Math.Abs(value)));

		//    if (roundingDigits > 0)
		//        return Utilities.MidpointRoundingAwayFromZero(value, roundingDigits);

		//    return value;
		//}

		//#endregion // RoundToExcelDisplayValue

		#endregion // Moved
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