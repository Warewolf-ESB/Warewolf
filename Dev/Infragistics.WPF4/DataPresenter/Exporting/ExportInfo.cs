using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Windows.DataPresenter
{
	/// <summary>
	/// Object used to provide information about an export operation
	/// </summary>
	[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
	public class ExportInfo : PropertyChangeNotifier
	{
		#region Member Variables

		private string _exportType;
		private string _fileName;
		private string _fileNameWithoutPath;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ExportInfo"/>
		/// </summary>
		public ExportInfo()
		{
		} 
		#endregion //Constructor

		#region Properties
		/// <summary>
		/// Returns or sets a string indicating the type of the export (e.g. Word).
		/// </summary>
		public string ExportType
		{
			get { return _exportType; }
			set { this.SetField(ref _exportType, value, "ExportType"); }
		} 

		/// <summary>
		/// Returns or sets the name of the file to which the export is being saved.
		/// </summary>
		public string FileName
		{
			get { return _fileName; }
			set 
			{
				if (this.SetField(ref _fileName, value, "FileName"))
				{
					string withoutPath = value == null ? null : value;

					try
					{
						withoutPath = System.IO.Path.GetFileName(value);
					}
					catch (ArgumentException)
					{
					}

					this.FileNameWithoutPath = withoutPath;
				}
			}
		}

		/// <summary>
		/// Returns or sets the name of the file excluding path information.
		/// </summary>
		public string FileNameWithoutPath
		{
			get { return _fileNameWithoutPath; }
			private set { this.SetField(ref _fileNameWithoutPath, value, "FileNameWithoutPath"); }
		}
		#endregion //Properties

		#region Methods

		#region SetField
		private bool SetField<T>(ref T member, T value, string propertyName)
		{
			if (EqualityComparer<T>.Default.Equals(member, value))
				return false;

			member = value;
			this.OnPropertyChanged(propertyName);
			return true;
		}
		#endregion //SetField

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