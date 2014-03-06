using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Infragistics.Documents.Excel.Serialization;
using Infragistics.Documents.Excel.Serialization.BIFF8.EscherRecords;
using Infragistics.Documents.Excel.Serialization.Excel2007;







using System.Drawing;
using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// Represents any image on the worksheet except the background image.
	/// </summary>
	/// <seealso cref="Worksheet.ImageBackground"/>



	public

		 class WorksheetImage : WorksheetShape
		, IWorksheetImage		// MD 10/30/11 - TFS90733
	{
		#region Member Variables

		private Image image;

		#endregion Member Variables

		#region Constructor

		internal WorksheetImage() { }

		// MD 1/6/12 - TFS92740
		internal WorksheetImage(bool initializeDefaults) :
			base(initializeDefaults) { }



#region Infragistics Source Cleanup (Region)



























#endregion // Infragistics Source Cleanup (Region)

		/// <summary>
		/// Creates a new <see cref="WorksheetImage"/> instance.
		/// </summary>
		/// <param name="image">The image displayed in the worksheet for this shape.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="image"/> is null.
		/// </exception>
		public WorksheetImage( Image image )
		{
			if ( image == null )
				throw new ArgumentNullException( "image", SR.GetString( "LE_ArgumentNullException_Image" ) );

			this.image = image;
		}

		#endregion Constructor

		#region Interfaces

		// MD 10/30/11 - TFS90733
		#region IWorksheetImage Members

		Image IWorksheetImage.Image
		{
			get { return this.Image; }
			set { this.Image = value; }
		}

		#endregion

		#endregion // Interfaces

		#region Base Class Overrides

		#region PopuplateDrawingProperties

		internal override void PopuplateDrawingProperties( WorkbookSerializationManager manager )
		{
			base.PopuplateDrawingProperties( manager );

			// If we already have a drawing properties collection, remove the BLIP id property,
			// we need to recreate it because the BLIP index may have changed
			if ( this.DrawingProperties1 != null )
			{
				for ( int i = this.DrawingProperties1.Count - 1; i >= 0; i-- )
				{
					if ( this.DrawingProperties1[ i ].IsBlipId )
						this.DrawingProperties1.RemoveAt( i );
				}
			}
			else
			{
				this.DrawingProperties1 = new List<PropertyTableBase.PropertyValue>();
			}

			// Find the index of the image in the serialization manager's collection of images
			int index = manager.Images.IndexOf( new WorkbookSerializationManager.ImageHolder( this.Image ) );

			// The image should have been found
			Debug.Assert( index >= 0 );
			if ( index >= 0 )
			{
				// Add the BLIP id property to the property table for the shape
				this.DrawingProperties1.Add( new PropertyTableBase.PropertyValue(
					PropertyType.BLIPId,
					(uint)( index + 1 ),
					false,
					true ) );

				// MD 9/2/08 - Cell Comments
				// The properties are now sorted by the caller.
				//// Sort the properties because they should be saved in order.
				//this.DrawingProperties1.Sort();
			}
		}

		#endregion PopuplateDrawingProperties

		// MD 10/10/11 - TFS90805
		#region Removed

		//// MD 7/20/2007 - BR25039
		//#region Type

		//internal override ShapeType Type
		//{
		//    get { return ShapeType.PictureFrame; }
		//}

		//#endregion Type

		#endregion  // Removed

		// MD 10/10/11 - TFS90805
		#region Type2003

		internal override ShapeType? Type2003
		{
			get { return ShapeType.PictureFrame; }
		}

		#endregion  // Type2003

		// MD 10/10/11 - TFS90805
		#region Type2007

		internal override ST_ShapeType? Type2007
		{
			get { return null; }
		}

		#endregion  // Type2007

		#endregion Base Class Overrides

		#region Methods

		#region SetBoundsInTwips

		/// <summary>
		/// Sets the bounds of the shape in twips (1/20th of a point).
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The shape will only be position at the specified bounds while the worksheet remains in the current configuration.
		/// If any rows or columns before or within the shape are resized, the shape will no longer be placed at the bounds specified.
		/// </p>
		/// <p class="body">
		/// If <paramref name="maintainAspectRatio"/> is False, this just calls <see cref="WorksheetShape.SetBoundsInTwips(Worksheet,Rectangle)"/> on its 
		/// base class.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="worksheet"/> is null.
		/// </exception>
		/// <param name="worksheet">The worksheet on which the shape should be placed.</param>
		/// <param name="bounds">The new bounds where the shape should be placed.</param> 
		/// <param name="maintainAspectRatio">The value indicating whether the image's aspect ratio should be maintained.</param>
		public void SetBoundsInTwips( Worksheet worksheet, Rectangle bounds, bool maintainAspectRatio )
		{
			// MD 3/24/10 - TFS28374
			// Moved all code to a new overload.
			this.SetBoundsInTwips(worksheet, bounds, maintainAspectRatio, PositioningOptions.None);
		}

		// MD 3/24/10 - TFS28374
		// Added a new overload.
		/// <summary>
		/// Sets the bounds of the shape in twips (1/20th of a point).
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The shape will only be position at the specified bounds while the worksheet remains in the current configuration.
		/// If any rows or columns before or within the shape are resized, the shape will no longer be placed at the bounds specified.
		/// </p>
		/// <p class="body">
		/// If <paramref name="maintainAspectRatio"/> is False, this just calls <see cref="WorksheetShape.SetBoundsInTwips(Worksheet,Rectangle)"/> on its 
		/// base class.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="worksheet"/> is null.
		/// </exception>
		/// <param name="worksheet">The worksheet on which the shape should be placed.</param>
		/// <param name="bounds">The new bounds where the shape should be placed.</param> 
		/// <param name="maintainAspectRatio">The value indicating whether the image's aspect ratio should be maintained.</param>
		/// <param name="options">The options to use when setting the bounds of the shape.</param>
		public void SetBoundsInTwips(Worksheet worksheet, Rectangle bounds, bool maintainAspectRatio, PositioningOptions options)
		{
			if ( maintainAspectRatio == false )
			{
				// MD 3/24/10 - TFS28374
				// Use the new overload which takes an options parameter.
				//this.SetBoundsInTwips( worksheet, bounds );
				this.SetBoundsInTwips(worksheet, bounds, options);
				return;
			}

			float aspectRatio = this.image.Height / (float)this.image.Width;

			Size heightSize = new Size( (int)( bounds.Height / aspectRatio ), bounds.Height );
			Size widthSize = new Size( bounds.Width, (int)( bounds.Width * aspectRatio ) );

			Debug.Assert(
				heightSize == widthSize ||
				heightSize.Width > bounds.Width ||
				widthSize.Height > bounds.Height );

			Size resolvedSize;

			if ( heightSize.Width <= bounds.Width )
				resolvedSize = heightSize;
			else
				resolvedSize = widthSize;

			Point resolvedLocation = new Point(
				bounds.X + ( bounds.Width - resolvedSize.Width ) / 2,
				bounds.Y + ( bounds.Height - resolvedSize.Height ) / 2 );

			// MD 3/24/10 - TFS28374
			// Use the new overload which takes an options parameter.
			//this.SetBoundsInTwips( worksheet, new Rectangle( resolvedLocation, resolvedSize ) );
			this.SetBoundsInTwips(worksheet, new Rectangle(resolvedLocation, resolvedSize), options);
		}

		#endregion SetBoundsInTwips

		#endregion Methods

		#region Properties

		#region Image

		/// <summary>
		/// Gets the image displayed in the worksheet for this shape.
		/// </summary>
		/// <value>The image displayed in the worksheet for this shape.</value>



        public Image Image

        {
			get { return this.image; }

            internal 

            set { this.image = value; }
		}

		#endregion Image

		#endregion Properties
	}

	// MD 10/30/11 - TFS90733
	// Some other shapes (such as controls) can store images, so use an interface for all shape types that may have an image.
	internal interface IWorksheetImage
	{
		Image Image { get; set; }
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