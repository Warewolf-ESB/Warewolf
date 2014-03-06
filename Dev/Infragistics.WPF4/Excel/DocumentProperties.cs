using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;







using Infragistics.Documents.Excel.StructuredStorage.FileTypes;
using Infragistics.Documents.Excel.StructuredStorage;

namespace Infragistics.Documents.Excel

{
	

	/// <summary>
	/// Class which exposes the document level properties for a Microsoft Excel file.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// The properties exposed by this class can be changed on a Microsoft Excel file by right-clicking it
	/// in Windows Explorer and editing the properties on the Summary tab.
	/// </p>
	/// </remarks>



	public

		 class DocumentProperties
	{
		#region Member Variables

		private string author;
		private string category;
		private string comments;
		private string company;	// MD 6/11/07 - BR23729
		private string keywords;
		private string manager;	// MD 6/11/07 - BR23729
		private string status;
		private string subject;
		private string title;

		#endregion Member Variables

		#region Constructor

		internal DocumentProperties() { }

		#endregion Constructor

		#region Internal Methods

		#region Load

		internal void Load( StructuredStorageManager manager )
		{
			DocumentSummaryInformation documentSummaryInformation = new DocumentSummaryInformation();
			SummaryInformation summaryInformation = new SummaryInformation();

			// MD 10/1/08 - TFS8453
			// Moved this string to a constant
			//Stream documentSummaryInfoStream = manager.GetFileStream( "DocumentSummaryInformation" );
			Stream documentSummaryInfoStream = manager.GetFileStream( Workbook.Excel2003StructuredStorageDocumentSummaryInformationFileName );

			if ( documentSummaryInfoStream != null )
				documentSummaryInformation.Load( documentSummaryInfoStream );

			// MD 10/1/08 - TFS8453
			// Moved this string to a constant
			//Stream summaryInfoStream = manager.GetFileStream( "SummaryInformation" );
			Stream summaryInfoStream = manager.GetFileStream( Workbook.Excel2003StructuredStorageSummaryInformationFileName );

			if ( summaryInfoStream != null )
				summaryInformation.Load( summaryInfoStream );

			// Get all the values first, then apply them, because if there was some problem reading the stream,
			// we don't want bad data being populated here
			string newAuthor = (string)summaryInformation.Properties[ SummaryPropertyType.Author ];
			string newCategory = (string)documentSummaryInformation.Properties[ DocumentSummaryPropertyType.Category ];
			string newComments = (string)summaryInformation.Properties[ SummaryPropertyType.Comments ];
			string newCompany = (string)documentSummaryInformation.Properties[ DocumentSummaryPropertyType.Company ];	// MD 6/11/07 - BR23729
			string newKeywords = (string)summaryInformation.Properties[ SummaryPropertyType.Keywords ];
			string newManager = (string)documentSummaryInformation.Properties[ DocumentSummaryPropertyType.Manager ];	// MD 6/11/07 - BR23729
			string newSubject = (string)summaryInformation.Properties[ SummaryPropertyType.Subject ];
			string newStatus = (string)documentSummaryInformation.Properties[ DocumentSummaryPropertyType.Status ];
			string newTitle = (string)summaryInformation.Properties[ SummaryPropertyType.Title ];

			this.author = newAuthor;
			this.category = newCategory;
			this.comments = newComments;
			this.company = newCompany;	// MD 6/11/07 - BR23729
			this.keywords = newKeywords;
			this.manager = newManager;	// MD 6/11/07 - BR23729
			this.subject = newSubject;
			this.status = newStatus;
			this.title = newTitle;
		}

		#endregion Load

		#region Save

		internal void Save( StructuredStorageManager manager )
		{
			SummaryInformation summaryInformation = new SummaryInformation();
			DocumentSummaryInformation documentSummaryInformation = new DocumentSummaryInformation();

			summaryInformation.Properties[ SummaryPropertyType.Author ] = this.author;
			documentSummaryInformation.Properties[ DocumentSummaryPropertyType.Category ] = this.category;
			summaryInformation.Properties[ SummaryPropertyType.Comments ] = this.comments;
			documentSummaryInformation.Properties[ DocumentSummaryPropertyType.Company ] = this.company;	// MD 6/11/07 - BR23729
			summaryInformation.Properties[ SummaryPropertyType.Keywords ] = this.keywords;
			documentSummaryInformation.Properties[ DocumentSummaryPropertyType.Manager ] = this.manager;	// MD 6/11/07 - BR23729
			summaryInformation.Properties[ SummaryPropertyType.Subject ] = this.subject;
			documentSummaryInformation.Properties[ DocumentSummaryPropertyType.Status ] = this.status;
			summaryInformation.Properties[ SummaryPropertyType.Title ] = this.title;

			// MD 10/1/08 - TFS8453
			// Moved this string to a constant
			//Stream documentSummaryInfoStream = manager.AddFile( "DocumentSummaryInformation" );
			Stream documentSummaryInfoStream = manager.AddFile( Workbook.Excel2003StructuredStorageDocumentSummaryInformationFileName );

			if ( documentSummaryInfoStream != null )
				documentSummaryInformation.Save( documentSummaryInfoStream );

			// MD 10/1/08 - TFS8453
			// Moved this string to a constant
			//Stream summaryInfoStream = manager.AddFile( "SummaryInformation" );
			Stream summaryInfoStream = manager.AddFile( Workbook.Excel2003StructuredStorageSummaryInformationFileName );

			if ( summaryInfoStream != null )
				summaryInformation.Save( summaryInfoStream );
		}

		#endregion Save

		#endregion Internal Methods

		#region Properties

		#region Author

		/// <summary>
		/// Gets or sets the author of the document.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The value of the property has no effect on the contents of the file when opened in a host application.
		/// It is simply extra data associated with the document.
		/// </p>
		/// </remarks>
		/// <value>A string specifying the author of the document.</value>
		public string Author
		{
			get { return this.author; }
			set { this.author = value; }
		}

		#endregion Author

		#region Category

		/// <summary>
		/// Gets or sets the category of the document.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The value of the property has no effect on the contents of the file when opened in a host application.
		/// It is simply extra data associated with the document.
		/// </p>
		/// </remarks>
		/// <value>A string specifying the category of the document.</value>
		public string Category
		{
			get { return this.category; }
			set { this.category = value; }
		}

		#endregion Category

		#region Comments

		/// <summary>
		/// Gets or sets the comments associated with the document.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The value of the property has no effect on the contents of the file when opened in a host application.
		/// It is simply extra data associated with the document.
		/// </p>
		/// </remarks>
		/// <value>A string specifying the comments associated with the document.</value>
		public string Comments
		{
			get { return this.comments; }
			set { this.comments = value; }
		}

		#endregion Comments

		// MD 6/11/07 - BR23729
		// Added property
		#region Company

		/// <summary>
		/// Gets or sets the company to which the document belongs.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The value of the property has no effect on the contents of the file when opened in a host application.
		/// It is simply extra data associated with the document.
		/// </p>
		/// </remarks>
		/// <value>A string specifying the company to which the document belongs.</value>
		public string Company
		{
			get { return this.company; }
			set { this.company = value; }
		}

		#endregion Company

		#region Keywords

		/// <summary>
		/// Gets or sets the keywords which describe the document.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The value of the property has no effect on the contents of the file when opened in a host application.
		/// It is simply extra data associated with the document.
		/// </p>
		/// </remarks>
		/// <value>A string specifying the keywords which describe the document.</value>
		public string Keywords
		{
			get { return this.keywords; }
			set { this.keywords = value; }
		}

		#endregion Keywords

		// MD 6/11/07 - BR23729
		// Added property
		#region Manager

		/// <summary>
		/// Gets or sets the manager associated with the document.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The value of the property has no effect on the contents of the file when opened in a host application.
		/// It is simply extra data associated with the document.
		/// </p>
		/// </remarks>
		/// <value>A string specifying the manager associated with the document.</value>
		public string Manager
		{
			get { return this.manager; }
			set { this.manager = value; }
		}

		#endregion Manager

		#region Status

		/// <summary>
		/// Gets or sets the current status of the document.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The value of the property has no effect on the contents of the file when opened in a host application.
		/// It is simply extra data associated with the document.
		/// </p>
		/// </remarks>
		/// <value>A string representing the current status of the document.</value>
		public string Status
		{
			get { return this.status; }
			set { this.status = value; }
		}

		#endregion Status

		#region Subject

		/// <summary>
		/// Gets or sets the subject of the contents of the document.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The value of the property has no effect on the contents of the file when opened in a host application.
		/// It is simply extra data associated with the document.
		/// </p>
		/// </remarks>
		/// <value>A string specifying the subject of the contents of the document.</value>
		public string Subject
		{
			get { return this.subject; }
			set { this.subject = value; }
		}

		#endregion Subject

		#region Title

		/// <summary>
		/// Gets or sets the title of the document.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The value of the property has no effect on the contents of the file when opened in a host application.
		/// It is simply extra data associated with the document.
		/// </p>
		/// </remarks>
		/// <value>A string specifying the title of the document.</value>
		public string Title
		{
			get { return this.title; }
			set { this.title = value; }
		}

		#endregion Title

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