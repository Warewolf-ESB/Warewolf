using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Infragistics.Windows.Design.SmartTagFramework
{
	/// <summary>
	/// Exposes static properties for URIs, ImageSources etc that point to various pieces of content contained in the assembly.
	/// </summary>
	public static class ContentLocators
	{
		#region Member Variables

		private static Assembly				_assembly;

		#endregion //Member Variables

		#region Properties

			#region Private Properties

				#region Assembly

		private static Assembly Assembly
		{
			get
			{
				if (ContentLocators._assembly == null)
					ContentLocators._assembly = Type.GetType("Infragistics.Windows.Design.SmartTagFramework.GenericAdorner").Assembly;

				return ContentLocators._assembly;
			}
		}

			#endregion //Assembly

			#endregion //Private Properties

			#region Public Properties
    
				#region ImageSourceCloseButtonGif

		/// <summary>
		/// ImageSource for SmartTagFramework\Images\CloseButton.gif
		/// </summary>
		public static ImageSource ImageSourceCloseButtonGif
		{
			get { return ContentLocators.BuildImageSource("SmartTagFramework/Images/CloseButton.gif"); }
		}

				#endregion //ImageSourceCloseButtonGif	
    
				#region ImageSourceDrillIntoPng

		/// <summary>
		/// ImageSource for SmartTagFramework\Images\DrillInto.png
		/// </summary>
		public static ImageSource ImageSourceDrillIntoPng
		{
			get { return ContentLocators.BuildImageSource("SmartTagFramework/Images/DrillInto.png"); }
		}

				#endregion //ImageSourceDrillIntoPng

				#region ImageSourceFolderJpg

		/// <summary>
		/// ImageSource for SmartTagFramework\Images\Folder.jpg
		/// </summary>
		public static ImageSource ImageSourceFolderJpg
		{
			get { return ContentLocators.BuildImageSource("SmartTagFramework/Images/Folder.jpg"); }
		}

				#endregion //ImageSourceFolderJpg	
    
				#region ImageSourceInfoPng

		/// <summary>
		/// ImageSource for SmartTagFramework\Images\Info.Png
		/// </summary>
		public static ImageSource ImageSourceInfoPng
		{
			get { return ContentLocators.BuildImageSource("SmartTagFramework/Images/Info.Png"); }
		}

				#endregion //ImageSourceInfoPng	
    
				#region UriDesignerActionItemTemplatesXaml

		/// <summary>
		/// Uri for SmartTagFramework\ThemeResources\Generic\DesignerActionItemTemplates.xaml
		/// </summary>
		public static Uri UriDesignerActionItemTemplatesXaml
		{
			get { return Utilities.BuildEmbeddedResourceUri(ContentLocators.Assembly, @"SmartTagFramework\ThemeResources\Generic\DesignerActionItemTemplates.xaml"); }
		}

				#endregion //UriDesignerActionItemTemplatesXaml

				#region UriExpanderXaml

		/// <summary>
		/// Uri for SmartTagFramework\ThemeResources\Generic\Expander.xaml
		/// </summary>
		public static Uri UriExpanderXaml
		{
			get { return Utilities.BuildEmbeddedResourceUri(ContentLocators.Assembly, @"SmartTagFramework\ThemeResources\Generic\Expander.xaml"); }
		}

				#endregion //UriExpanderXaml

				#region UriGenericXaml

		/// <summary>
		/// Uri for Themes\generic.xaml
		/// </summary>
		public static Uri UriGenericXaml
		{
			get { return Utilities.BuildEmbeddedResourceUri(ContentLocators.Assembly, @"themes\generic.xaml"); }
		}

				#endregion //UriGenericXaml

				#region UriPropertyEditorsXaml

		/// <summary>
		/// Uri for SmartTagFramework\ThemeResources\Generic\PropertyEditors.xaml
		/// </summary>
		public static Uri UriPropertyEditorsXaml
		{
			get { return Utilities.BuildEmbeddedResourceUri(ContentLocators.Assembly, @"SmartTagFramework\ThemeResources\Generic\PropertyEditors.xaml"); }
		}

				#endregion //UriPropertyEditorsXaml

			#endregion //Public Properties

		#endregion //Properties

		#region Methods

			#region BuildImageSource

		private static ImageSource BuildImageSource(string resourcePath)
		{
			string			assemblyName	= ContentLocators.Assembly.FullName;
			StringBuilder	sb				= new StringBuilder(assemblyName.Length + 40 + resourcePath.Length);

			sb.Append("pack://application:,,,/");
			sb.Append(assemblyName);
			sb.Append(";component/");
			sb.Append(resourcePath);

			BitmapImage bitmapImage = new BitmapImage();
			bitmapImage.BeginInit();
			bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
			bitmapImage.UriSource	= new Uri(sb.ToString(), UriKind.Absolute);
			bitmapImage.EndInit();

			return bitmapImage;
		}

			#endregion //BuildImageSource

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