using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows;
using Infragistics.Documents.Word;
using Infragistics.Documents.Core;

namespace Infragistics.Windows.DataPresenter.WordWriter
{
	/// <summary>
	/// Exposes sections settings for the Word export.
	/// </summary>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class WordDocumentSettings : DependencyObject
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="WordDocumentSettings"/>
		/// </summary>
		public WordDocumentSettings()
		{
		}
		#endregion //Constructor

		#region Properties

		#region Application

		/// <summary>
		/// Identifies the <see cref="Application"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ApplicationProperty = DependencyProperty.Register("Application",
			typeof(string), typeof(WordDocumentSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the name of the application that created the document.
		/// </summary>
		/// <seealso cref="ApplicationProperty"/>
		/// <seealso cref="OfficeDocumentProperties.Application"/>
		[Bindable(true)]
		public string Application
		{
			get
			{
				return (string)this.GetValue(WordDocumentSettings.ApplicationProperty);
			}
			set
			{
				this.SetValue(WordDocumentSettings.ApplicationProperty, value);
			}
		}

		#endregion //Application

		#region Author

		/// <summary>
		/// Identifies the <see cref="Author"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AuthorProperty = DependencyProperty.Register("Author",
			typeof(string), typeof(WordDocumentSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the author of the document.
		/// </summary>
		/// <seealso cref="AuthorProperty"/>
		/// <seealso cref="OfficeDocumentProperties.Author"/>
		[Bindable(true)]
		public string Author
		{
			get
			{
				return (string)this.GetValue(WordDocumentSettings.AuthorProperty);
			}
			set
			{
				this.SetValue(WordDocumentSettings.AuthorProperty, value);
			}
		}

		#endregion //Author

		#region Category

		/// <summary>
		/// Identifies the <see cref="Category"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register("Category",
			typeof(string), typeof(WordDocumentSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the category for the document.
		/// </summary>
		/// <seealso cref="CategoryProperty"/>
		/// <seealso cref="OfficeDocumentProperties.Category"/>
		[Bindable(true)]
		public string Category
		{
			get
			{
				return (string)this.GetValue(WordDocumentSettings.CategoryProperty);
			}
			set
			{
				this.SetValue(WordDocumentSettings.CategoryProperty, value);
			}
		}

		#endregion //Category

		#region Comments

		/// <summary>
		/// Identifies the <see cref="Comments"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CommentsProperty = DependencyProperty.Register("Comments",
			typeof(string), typeof(WordDocumentSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the comments for the document.
		/// </summary>
		/// <seealso cref="CommentsProperty"/>
		/// <seealso cref="OfficeDocumentProperties.Comments"/>
		[Bindable(true)]
		public string Comments
		{
			get
			{
				return (string)this.GetValue(WordDocumentSettings.CommentsProperty);
			}
			set
			{
				this.SetValue(WordDocumentSettings.CommentsProperty, value);
			}
		}

		#endregion //Comments

		#region Company

		/// <summary>
		/// Identifies the <see cref="Company"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CompanyProperty = DependencyProperty.Register("Company",
			typeof(string), typeof(WordDocumentSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the company for the document.
		/// </summary>
		/// <seealso cref="CompanyProperty"/>
		/// <seealso cref="OfficeDocumentProperties.Company"/>
		[Bindable(true)]
		public string Company
		{
			get
			{
				return (string)this.GetValue(WordDocumentSettings.CompanyProperty);
			}
			set
			{
				this.SetValue(WordDocumentSettings.CompanyProperty, value);
			}
		}

		#endregion //Company

		#region ComplexScriptCulture

		/// <summary>
		/// Identifies the <see cref="ComplexScriptCulture"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComplexScriptCultureProperty = DependencyProperty.Register("ComplexScriptCulture",
			typeof(string), typeof(WordDocumentSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the language used to check spelling and grammar when processing text runs which contain complex script characters.
		/// </summary>
		/// <seealso cref="ComplexScriptCultureProperty"/>
		/// <seealso cref="WordDocumentProperties.ComplexScriptCulture"/>
		[Bindable(true)]
		public string ComplexScriptCulture
		{
			get
			{
				return (string)this.GetValue(WordDocumentSettings.ComplexScriptCultureProperty);
			}
			set
			{
				this.SetValue(WordDocumentSettings.ComplexScriptCultureProperty, value);
			}
		}

		#endregion //ComplexScriptCulture

		#region EastAsiaCulture

		/// <summary>
		/// Identifies the <see cref="EastAsiaCulture"/> dependency property
		/// </summary>
		public static readonly DependencyProperty EastAsiaCultureProperty = DependencyProperty.Register("EastAsiaCulture",
			typeof(string), typeof(WordDocumentSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the language used to check spelling and grammar when processing text runs which contain East Asian characters.
		/// </summary>
		/// <seealso cref="EastAsiaCultureProperty"/>
		/// <seealso cref="WordDocumentProperties.EastAsiaCulture"/>
		[Bindable(true)]
		public string EastAsiaCulture
		{
			get
			{
				return (string)this.GetValue(WordDocumentSettings.EastAsiaCultureProperty);
			}
			set
			{
				this.SetValue(WordDocumentSettings.EastAsiaCultureProperty, value);
			}
		}

		#endregion //EastAsiaCulture

		#region Keywords

		/// <summary>
		/// Identifies the <see cref="Keywords"/> dependency property
		/// </summary>
		public static readonly DependencyProperty KeywordsProperty = DependencyProperty.Register("Keywords",
			typeof(string), typeof(WordDocumentSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the keywords for the document.
		/// </summary>
		/// <seealso cref="KeywordsProperty"/>
		/// <seealso cref="OfficeDocumentProperties.Keywords"/>
		[Bindable(true)]
		public string Keywords
		{
			get
			{
				return (string)this.GetValue(WordDocumentSettings.KeywordsProperty);
			}
			set
			{
				this.SetValue(WordDocumentSettings.KeywordsProperty, value);
			}
		}

		#endregion //Keywords

		#region LatinCulture

		/// <summary>
		/// Identifies the <see cref="LatinCulture"/> dependency property
		/// </summary>
		public static readonly DependencyProperty LatinCultureProperty = DependencyProperty.Register("LatinCulture",
			typeof(string), typeof(WordDocumentSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the language used to check spelling and grammar when processing text runs which contain Latin characters.
		/// </summary>
		/// <seealso cref="LatinCultureProperty"/>
		/// <seealso cref="WordDocumentProperties.LatinCulture"/>
		[Bindable(true)]
		public string LatinCulture
		{
			get
			{
				return (string)this.GetValue(WordDocumentSettings.LatinCultureProperty);
			}
			set
			{
				this.SetValue(WordDocumentSettings.LatinCultureProperty, value);
			}
		}

		#endregion //LatinCulture

		#region Manager

		/// <summary>
		/// Identifies the <see cref="Manager"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ManagerProperty = DependencyProperty.Register("Manager",
			typeof(string), typeof(WordDocumentSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the manager for the document.
		/// </summary>
		/// <seealso cref="ManagerProperty"/>
		/// <seealso cref="OfficeDocumentProperties.Manager"/>
		[Bindable(true)]
		public string Manager
		{
			get
			{
				return (string)this.GetValue(WordDocumentSettings.ManagerProperty);
			}
			set
			{
				this.SetValue(WordDocumentSettings.ManagerProperty, value);
			}
		}

		#endregion //Manager

		#region Status

		/// <summary>
		/// Identifies the <see cref="Status"/> dependency property
		/// </summary>
		public static readonly DependencyProperty StatusProperty = DependencyProperty.Register("Status",
			typeof(string), typeof(WordDocumentSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the status for the document.
		/// </summary>
		/// <seealso cref="StatusProperty"/>
		/// <seealso cref="OfficeDocumentProperties.Status"/>
		[Bindable(true)]
		public string Status
		{
			get
			{
				return (string)this.GetValue(WordDocumentSettings.StatusProperty);
			}
			set
			{
				this.SetValue(WordDocumentSettings.StatusProperty, value);
			}
		}

		#endregion //Status

		#region Subject

		/// <summary>
		/// Identifies the <see cref="Subject"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SubjectProperty = DependencyProperty.Register("Subject",
			typeof(string), typeof(WordDocumentSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the subject for the document.
		/// </summary>
		/// <seealso cref="SubjectProperty"/>
		/// <seealso cref="OfficeDocumentProperties.Subject"/>
		[Bindable(true)]
		public string Subject
		{
			get
			{
				return (string)this.GetValue(WordDocumentSettings.SubjectProperty);
			}
			set
			{
				this.SetValue(WordDocumentSettings.SubjectProperty, value);
			}
		}

		#endregion //Subject

		#region Title

		/// <summary>
		/// Identifies the <see cref="Title"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title",
			typeof(string), typeof(WordDocumentSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the title for the document.
		/// </summary>
		/// <seealso cref="TitleProperty"/>
		/// <seealso cref="OfficeDocumentProperties.Title"/>
		[Bindable(true)]
		public string Title
		{
			get
			{
				return (string)this.GetValue(WordDocumentSettings.TitleProperty);
			}
			set
			{
				this.SetValue(WordDocumentSettings.TitleProperty, value);
			}
		}

		#endregion //Title

		#endregion //Properties

		#region Methods

		#region Initialize
		internal void Initialize(WordDocumentWriter writer, WordDocumentProperties documentProperties)
		{
			UnitOfMeasurement unit = writer.Unit;
			documentProperties.Author = this.Author ?? documentProperties.Author;
			documentProperties.Category = this.Category ?? documentProperties.Category;
			documentProperties.Comments = this.Comments ?? documentProperties.Comments;
			documentProperties.Company = this.Company ?? documentProperties.Company;
			documentProperties.Keywords = this.Keywords ?? documentProperties.Keywords;
			documentProperties.Manager = this.Manager ?? documentProperties.Manager;
			documentProperties.Subject = this.Subject ?? documentProperties.Subject;
			documentProperties.Status = this.Status ?? documentProperties.Status;
			documentProperties.Title = this.Title ?? documentProperties.Title;

			// the following have default properties so only set them if we have them
			if (null != this.Application)
				documentProperties.Application = this.Application;

			if (null != this.ComplexScriptCulture)
				documentProperties.ComplexScriptCulture = this.ComplexScriptCulture;
			if (null != this.EastAsiaCulture)
				documentProperties.EastAsiaCulture = this.EastAsiaCulture;
			if (null != this.LatinCulture)
				documentProperties.LatinCulture = this.LatinCulture;
		}
		#endregion //Initialize

		#endregion //Methods
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