using System;
using System.Diagnostics;
using System.Collections;
using System.Globalization;
using Infragistics.Documents.Excel.FormulaUtilities.CalcEngine;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel.CalcEngine
{
	#region RefBase

	internal abstract class RefBase : IExcelCalcReference
	{
		#region Constructor
		public RefBase()
		{
		}
		#endregion //Constructor

		#region Properties







		public virtual object Context { get{ return null;} }

		#endregion //Properties

		#region Methods

		#region  Implementation of IUltraCalcReference

		/// <summary>
		/// The fully qualified unique name for the referenced element.  Read Only.
		/// </summary>
		public virtual string AbsoluteName
		{
			get 
			{
				return null;
			}
		}

		// SSP 9/7/04
		// Added NormalizedAbsoluteName property as a part of case insensitive absolute
		// name implementation.
		//
		#region NormalizedAbsoluteName

		/// <summary>
		/// Returns the normalized absolute name. Calculation engine makes use of normalized
		/// absolute names of references to compare two references and search for references.
		/// This property should return the absolute name with case insensitive parts of
		/// the absolute names converted to lower case.
		/// </summary>
		public abstract string NormalizedAbsoluteName { get; }

		#endregion // NormalizedAbsoluteName

		/// <summary>
		/// The unqualified name of this referenced element.  Used to construct the fully qualified name by the
		/// <b>AbsoluteName</b> method. Read Only.
		/// </summary>
		/// <seealso cref="AbsoluteName"/>
		public abstract string ElementName {get;}

		public virtual ExcelCalcValue Value
		{
			get
			{
				return new ExcelCalcValue(new ExcelCalcErrorValue(ExcelCalcErrorCode.Value));
			}

			set
			{
				throw new UltraCalcException( SR.GetString( "Error_ReadOnlyReference",new Object[] {GetType().ToString() + " " + ElementName}) );
			}
		}

		public virtual ExcelCalcFormula Formula
		{
			get
			{
				return null;
			}
		}

		/// <summary>
		/// Create a new reference relative to this reference.
		/// </summary>
		/// <param name="inReference">The reference string.</param>
		/// <returns>The new reference.</returns>
		public abstract IExcelCalcReference CreateReference(string inReference);

		/// <summary>
		/// Return a collection of references if this Reference is enumerable.  An exception is
		/// thrown if this reference is not enumerable.
		/// </summary>
		/// <returns>A Reference collection.</returns>
		public virtual IExcelCalcReferenceCollection References
		{
			get
			{
				return null;
			}
		}

		/// <summary>
		/// Returns true if this reference contains inReference
		/// </summary>
		/// <param name="inReference">The contained candidate.</param>
		/// <returns>true if inReference is contained by this reference.</returns>
		public abstract bool ContainsReference(IExcelCalcReference inReference);

		/// <summary>
		/// Returns true if inReference is a proper subset of this reference
		/// </summary>
		/// <param name="inReference">The subset candidate.</param>
		/// <returns>true if inReference is contained by this reference.</returns>
		public abstract bool IsSubsetReference(IExcelCalcReference inReference);

		/// <summary>
		/// Returns true if this reference is enumerable.
		/// </summary>
		public virtual bool IsEnumerable 
		{ 
			get 
			{
				return false;
			}
		}

		#endregion

		/// <summary>
		/// Returns a string representation of the reference based on the reference type and absolute name.
		/// </summary>
        /// <returns>A string representation of the reference based on the reference type and absolute name.</returns>
        public override string ToString()
		{
			return this.GetType().ToString() + "-" + this.AbsoluteName;
		}

		// SSP 10/12/04
		// Overrode GetHashCode and Equals methods.
		//
		#region GetHashCode

		/// <summary>
		/// Overridden. Default implementation returns the hash code of the normalized absolute name.
		/// These objects are going to be added to data structure that make use of GetHashCode and 
		/// Equals methods. What this means is that the normalized absolute name must not change 
		/// throughout the lifetime of this instance. If it does change then the overriding class 
		/// must override this method and return a hash code based on some other immutable property 
		/// of the instance.
		/// </summary>
        /// <returns>A hash code of the normalized absolute name.</returns>
		public override int GetHashCode( )
		{
			return this.NormalizedAbsoluteName.GetHashCode( );
		}

		#endregion // GetHashCode

		#region Equals

		/// <summary>
		/// Overriden. Default implementation returns true if the normalized absolute names of this
		/// reference and the passed in reference match. These objects are going to be added to data 
		/// structure that make use of GetHashCode and Equals methods. What this means is that the 
		/// normalized absolute name must not change throughout the lifetime of this instance. If it 
		/// does change then the overriding class must override this method and return a hash code 
		/// based on some other immutable property of the instance.
		/// </summary>
        /// <param name="obj">The reference to compare with this instance.</param>
        /// <returns>True if the normalized absolute names of this reference and the passed in reference match</returns>
		public override bool Equals( object obj )
		{
			IExcelCalcReference reference = obj as IExcelCalcReference;
			return null != reference && reference.NormalizedAbsoluteName.Equals( this.NormalizedAbsoluteName );
		}

		#endregion // Equals

		#endregion //Methods

		#region IUltraCalcReference Members

		IExcelCalcFormula IExcelCalcReference.Formula
		{
			get { return this.Formula; }
		}

		#endregion
	}

	#endregion

	#region UCReference



#region Infragistics Source Cleanup (Region)






























#endregion // Infragistics Source Cleanup (Region)

	internal class UCReference : IExcelCalcReference
	{
		#region Member Variables

		private string referenceName;			// Storage for the reference string within the formula

		private IExcelCalcReference reference;	// Storage for the underlying object's reference instance
		private bool connected;					// Storage for boolean that denotes whether the object is connected or disconnected


		// MD 10/7/09 - TFS21185
		private string originalReferenceName;

		#endregion //Member Variables

		#region Constructor
		private UCReference(bool isConnected)
		{
			this.connected = isConnected;
		}

		/// <summary>
		/// Reference name constructor
		/// </summary>
		/// <param name="referenceName">Name of reference entered into formula used to create the underlying object reference</param>
		public UCReference(string referenceName) : this(false)
		{
			this.referenceName = referenceName == null ? null : referenceName.ToLower(); 

			// MD 10/7/09 - TFS21185
			// Cache the original reference name.
			this.originalReferenceName = this.referenceName;
		}

		// MD 7/25/08 - Excel formula solving





		internal UCReference(IExcelCalcReference reference)
			: this( true)
		{
			this.reference = reference;

			// MD 4/21/11 - TFS64442
			if (this.reference != null)
			{
				this.referenceName = this.reference.NormalizedAbsoluteName;
				this.originalReferenceName = this.reference.ElementName.ToLower();
			}
		}

		#endregion //Constructor

		#region IUltraCalcReference
		/// <summary>
		/// Returns a context for the Reference.
		/// </summary>
		/// <remarks><p class="body">Returns a meaningful object context for the reference. This could be an UltraGridCell, UltraGridColumn, NamedReference, Control, etc.</p></remarks>
		public object Context { get{ return null;} }

		/// <summary>
		/// Create a new reference relative to this reference.
		/// </summary>
		/// <param name="reference">The reference string.</param>
		/// <returns>The new reference.</returns>
		public IExcelCalcReference CreateReference(string reference)
		{
			if (this.connected)
				return this.reference.CreateReference(reference);
			else
				return new UCReference(reference);
		}

		/// <summary>
		/// Returns the absolute name for the reference
		/// </summary>
		public string AbsoluteName
		{
			get
			{
				if (this.connected)
					return this.reference.AbsoluteName;
				else
					return referenceName;
			}
		}

		// SSP 9/7/04
		// Added NormalizedAbsoluteName property as a part of case insensitive absolute
		// name implementation.
		//
		#region NormalizedAbsoluteName

		// MD 8/27/08 - Code Analysis - Performance
		//private string lastAbsoluteName = null;
		//private string lastNormalizedAbsoluteName = null;
		private string lastAbsoluteName;
		private string lastNormalizedAbsoluteName;

		/// <summary>
		/// Returns the normalized absolute name. UltraCalcEngine makes use normalized
		/// absolute names of references to compare two references and search for references.
		/// This property should return the absolute name with case insensitive parts of
		/// the absolute names converted to lower case.
		/// </summary>
		public string NormalizedAbsoluteName
		{
			get
			{
				string absoluteName = this.AbsoluteName;
				if ( this.lastAbsoluteName == absoluteName )
					return this.lastNormalizedAbsoluteName;

				this.lastAbsoluteName = absoluteName;
				this.lastNormalizedAbsoluteName = this.referenceName;

				return this.lastNormalizedAbsoluteName;
			}
		}

		#endregion // NormalizedAbsoluteName

		/// <summary>
		/// The unqualified name of this referenced element.  Used to construct the fully qualified name by the
		/// <b>AbsoluteName</b> method. Read Only.
		/// </summary>
		/// <seealso cref="AbsoluteName"/>
		public string ElementName
		{
			get
			{
				if (this.connected)
					return this.reference.ElementName;
				else
					return this.referenceName;
			}
		}
		/// <summary>
		/// The <b>UltraCalcValue</b>, if any, associated with this Reference.  If this reference 
		/// does not contain a Value then a <b>UltraCalcErrorValue</b> is returned.
		/// </summary>
		public ExcelCalcValue Value
		{
			get
			{
				if (this.connected)
					return this.reference.Value;
				else
				{
					// MD 7/14/08 - Excel formula solving
					// The reference error will now store the error code so it can override the error code represented.
					// If the reference is an  UltraCalcReferenceError, return its error code rather than always returning
					// a #REF! error.
					UltraCalcReferenceError referenceError = this.reference as UltraCalcReferenceError;

					if (referenceError != null)
						return ((IExcelCalcReference)referenceError).Value;

					// MD 8/22/08 - Excel formula solving
					// This would only happen with unresolved named references, so return a #NAME? error.
					return new ExcelCalcValue(new ExcelCalcErrorValue(ExcelCalcErrorCode.Name));
				}
			}
			set
			{ 
				if (this.connected)
					this.reference.Value = value;
			}
		}

		/// <summary>
		/// The <b>IUltraCalcFormula</b>, if any, associated with this Reference.  If this reference 
		/// can not contain a formula then null is returned.
		/// </summary>
		public IExcelCalcFormula Formula
		{
			get	
			{
				if (this.connected) 
					return this.reference.Formula;
				else
					return null; 	
			}
		}

		/// <summary>
		/// Return a collection of references if this Reference is enumerable.  An exception is
		/// thrown if this reference is not enumerable.
		/// </summary>
		/// <returns>A Reference collection.</returns>
		public IExcelCalcReferenceCollection References
		{
			get
			{
				if (this.connected)
					return this.reference.References;
				else
					return null;
			}
		}

		/// <summary>
		/// Returns true if this reference is enumerable.
		/// </summary>
		public bool IsEnumerable
		{
			get
			{
				if (this.connected)
					return this.reference.IsEnumerable;
				else
					return false;
			}
		}

		/// <summary>
		/// Returns true if this reference contains inReference
		/// </summary>
		/// <param name="reference">The contained candidate.</param>
		/// <returns>true if inReference is contained by this reference.</returns>
		public bool ContainsReference(IExcelCalcReference reference)
		{
			if (this.connected)
				return this.reference.ContainsReference(reference);
			else
				// SSP 9/7/04
				// Perform a case insensitive comparison.
				//
				//return String.Compare(referenceName, reference.AbsoluteName) == 0;
				return this.NormalizedAbsoluteName == reference.NormalizedAbsoluteName;
		}

		/// <summary>
		/// Returns true if inReference is a proper subset of this reference
		/// </summary>
		/// <param name="reference">The subset candidate.</param>
		/// <returns>true if inReference is contained by this reference.</returns>
		public bool IsSubsetReference(IExcelCalcReference reference)
		{
			if (this.connected)
				return this.reference.IsSubsetReference(reference);
			else
				return String.Compare(referenceName, reference.AbsoluteName) == 0;
		}

		#endregion // IUltraCalcReference

		#region Properties

		#region Reference

		/// <summary>
		/// Returns the underlying reference or this instance if the object is not connected.
		/// </summary>
		public virtual IExcelCalcReference Reference
		{
			get	
			{
				if (this.connected) 
					return this.reference;
				else
					return this; 	
			}
		}

		#endregion // Reference

		#region RootName

		/// <summary>
		/// Returns the root name for the reference
		/// </summary>
		public virtual string RootName
		{
			get 
			{ 
				if (AbsoluteName.StartsWith("//")) 
				{
					int len = AbsoluteName.IndexOf('/',2);
					string name = AbsoluteName.Substring(2,len == -1 ? AbsoluteName.Length-2 : len-2);
					return name;
				} 
				else 
				{
					// Illegal reference format
					//throw new UltraCalcErrorException(new UltraCalcErrorValue(UltraCalcErrorCode.Reference, AbsoluteName, this));
					//int len = AbsoluteName.IndexOf('/');
					//return AbsoluteName.Substring(0,len == -1 ? AbsoluteName.Length : len);
					return null;
				}
			}
		}

		#endregion //RootName

		#region IsConnected

		public bool IsConnected
		{
			get	{ return this.connected; }
		}

		#endregion //IsConnected

		#endregion //Properties

		#region Methods

		#region Connect

		public bool Connect(IExcelCalcReference reference)
		{
			//Debug.Assert(!connected);
			if (!this.connected)
			{

				// MD 10/7/09 - TFS21185
				// Restore the old reference name which was temporarily changed while we were disconnected.
				this.referenceName = this.originalReferenceName;
				{
					this.reference = reference.CreateReference(referenceName);

					// MD 10/7/09 - TFS21185
					// If a non-error reference is connected, store the absolute name as the reference name so that when we are later disconnected,
					// the absolute name of this reference will still return the correct absolute name.
                    // MRS 1/10/2011 - TFS62117
                    // Check for null
					//if ( ( this.reference is UltraCalcReferenceError ) == false )
                    if ( this.reference != null &&
                        (this.reference is UltraCalcReferenceError) == false)
					{
						this.referenceName = this.reference.AbsoluteName;
					}
				}

				if  (this.reference != null && !(this.reference is UltraCalcReferenceError))
				{
					this.connected = true;
					return true;
				}
				// SSP 10/12/04
				// Following code is not doing anything to the UCReference. It ends up being no op 
				// since the lines that assign referenceName are commented out.
				//
				
#region Infragistics Source Cleanup (Region)


























#endregion // Infragistics Source Cleanup (Region)

			}

			return false;
		}

		#endregion //Connect

		#region Disconnect

		public bool Disconnect( )
		{
			if (connected)
			{
				reference = null;
				connected = false;
				return true;
			}

			return false;
		}

		#endregion //Disconnect

		// SSP 10/12/04
		// Overrode GetHashCode and Equals methods.
		//
		#region GetHashCode

		/// <summary>
		/// Overridden. Default implementation returns the hash code of the normalized absolute name.
		/// These objects are going to be added to data structure that make use of GetHashCode and 
		/// Equals methods. What this means is that the normalized absolute name must not change 
		/// throughout the lifetime of this instance. If it does change then the overriding class 
		/// must override this method and return a hash code based on some other immutable property 
		/// of the instance.
		/// </summary>
        /// <returns>A hash code of the normalized absolute name.</returns>
		public override int GetHashCode( )
		{
			return this.originalReferenceName.GetHashCode();
		}

		#endregion // GetHashCode

		#region Equals

		/// <summary>
		/// Overriden. Default implementation returns true if the normalized absolute names of this
		/// reference and the passed in reference match. These objects are going to be added to data 
		/// structure that make use of GetHashCode and Equals methods. What this means is that the 
		/// normalized absolute name must not change throughout the lifetime of this instance. If it 
		/// does change then the overriding class must override this method and return a hash code 
		/// based on some other immutable property of the instance.
		/// </summary>
        /// <param name="obj">The object to test for equality.</param>
        /// <returns>True if the normalized absolute names of this instance and the passed in reference match.</returns>
		public override bool Equals( object obj )
		{
			if (!(obj is IExcelCalcReference))
				return false;

			UCReference uc = obj as UCReference;
			if ( null != uc && uc.connected && this.connected )
			{
				IExcelCalcReference r1 = this.reference;
				IExcelCalcReference r2 = uc.reference;
				return r1 == r2 || null != r1 && null != r2 && r1.Equals( r2 );
			}

			// If obj was not an UCReference then compare this UCReference' underlying reference to
			// the passed in reference.
			//
			// SSP 7/7/05
			// Just something I noticed. this.Reference can return itself if the reference is disconnected.
			// So to prevent infinite recursion check for this.connected.
			// 
			//if ( null == uc && null != this.Reference )
			if ( null == uc && this.connected && null != this.Reference )
				return this.Reference.Equals( obj );
						
			// Otherwise perform normalized absolute name comparision.
			//
			return this.NormalizedAbsoluteName == ((IExcelCalcReference)obj).NormalizedAbsoluteName;
		}

		#endregion // Equals

		#region RecacheName

		internal void RecacheName()
		{
			if (this.IsConnected)
			{
				this.referenceName = this.reference.AbsoluteName;
				this.originalReferenceName = this.reference.ElementName.ToLower();
			}
		}

		#endregion // RecacheName

		#endregion //Methods
	}

	#endregion // UCReference

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