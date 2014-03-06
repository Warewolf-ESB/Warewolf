using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Globalization;
using System.Security.Permissions;




using Infragistics.Shared;

namespace Infragistics.Documents.Excel

{
	/// <summary>
	/// The exception thrown when a formula parse error occurs.
	/// </summary>

	[Serializable]




	public

		 class FormulaParseException : Exception
	{
		#region Member Variables

		private int charIndexOfError;
		private string formulaValue;
		private string portionWithError;

		#endregion Member Variables

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="FormulaParseException"/> class.
		/// </summary>
		public FormulaParseException() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="FormulaParseException"/> class with a specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public FormulaParseException( string message )
			: base( message ) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="FormulaParseException"/> class with a specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="innerException">The exception that is the cause of this exception.</param>
		public FormulaParseException( string message, Exception innerException )
			: base( message, innerException ) { }


		/// <summary>
		/// Initializes a new instance of the <see cref="FormulaParseException"/> class with a specified error message 
		/// and information which helps determine the location of the parse error in the formula.
		/// </summary>
		/// <param name="charIndexOfError">The character index in the <paramref name="formulaValue"/> parameter where the parse error occurred.</param>
		/// <param name="formulaValue">The formula which had the error being parsed.</param>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="portionWithError">The portion of the formula which had the error being parsed.</param>
		public FormulaParseException( int charIndexOfError, string formulaValue, string message, string portionWithError )
			: this( message )
		{
			this.charIndexOfError = charIndexOfError;
			this.formulaValue = formulaValue;
			this.portionWithError = portionWithError;
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="FormulaParseException"/> class with the serialized data.
		/// </summary>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="info"/> is null.
		/// </exception>
		/// <param name="info">The serialized object data.</param>
		/// <param name="context">The context information about the source serialized stream.</param>
		protected FormulaParseException( SerializationInfo info, StreamingContext context )
			: base( info, context )
		{
			if ( info == null )
				throw new ArgumentNullException( "info" );

			this.charIndexOfError = info.GetInt32( "CharIndexOfError" );
			this.formulaValue = info.GetString( "FormulaValue" );
			this.portionWithError = info.GetString( "PortionWithError" );
		}

        #endregion Constructor

        #region Base Class Overrides

        #region GetObjectData

        /// <summary>
		/// Populates the specified <see cref="SerializationInfo"/> instance with this object's data.
		/// </summary>
		/// <param name="info">The serialized object data.</param>
		/// <param name="context">The context information about the destination serialized stream.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="info"/> is null.
		/// </exception>
		[System.Security.SecurityCritical]
		[SecurityPermission( SecurityAction.Demand, SerializationFormatter = true )]
		public override void GetObjectData( SerializationInfo info, StreamingContext context )
		{
			if ( info == null )
				throw new ArgumentNullException( "info" );

			info.AddValue( "CharIndexOfError", this.charIndexOfError );
			info.AddValue( "FormulaValue", this.formulaValue );
			info.AddValue( "PortionWithError", this.portionWithError );

			base.GetObjectData( info, context );
		}


		#endregion GetObjectData

		#endregion Base Class Overrides

		#region Properties

		#region CharIndexOfError

		/// <summary>
		/// Gets the character index in the <see cref="FormulaValue"/> at which the parse error occurred.
		/// </summary>
		/// <value>The character index in the FormulaValue at which the parse error occurred.</value>
		public int CharIndexOfError
		{
			get { return this.charIndexOfError; }
		}

		#endregion CharIndexOfError

		#region FormulaValue

		/// <summary>
		/// Gets the formula string which had the error being parsed.
		/// </summary>
		/// <value>The formula string which had the error being parsed.</value>
		public string FormulaValue
		{
			get { return this.formulaValue; }
		}

		#endregion FormulaValue

		#region Message

		/// <summary>
		/// Gets the error message and the portion of the formula with the error.
		/// </summary>
		public override string Message
		{
			get
			{
				string message = base.Message;

				if ( this.portionWithError == null )
					return message;

				string portionWithErrorDescriptionString = SR.GetString( "LE_FormulaParseException_Message_PortionWithError", this.portionWithError );

				if ( message == null )
					return portionWithErrorDescriptionString;

				return message + Environment.NewLine + portionWithErrorDescriptionString;
			}
		}

		#endregion Message

		#region PortionWithError

		/// <summary>
		/// Gets the portion of the formula which contains the error.
		/// </summary>
		public string PortionWithError
		{
			get { return this.portionWithError; }
		}

		#endregion PortionWithError

		#endregion Properties
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