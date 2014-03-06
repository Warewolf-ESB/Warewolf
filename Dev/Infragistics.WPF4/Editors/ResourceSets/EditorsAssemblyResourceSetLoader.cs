using System;
using System.Drawing.Design;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Markup;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Diagnostics;
using Infragistics.Windows;
using Infragistics.Windows.Themes;

namespace Infragistics.Windows.Themes.Internal
{
	/// <summary>
	/// Loads the themes contained in this assembly.
	/// </summary>
	public class EditorsAssemblyResourceSetLoader : AssemblyResourceSetLoader
	{
		/// <summary>
		/// Loads and registers the resource sets
		/// </summary>
		/// <param name="groupingName">The name of the grouping whose resources should be loaded or null if all resource sets should be loaded.</param>
		// AS 11/6/07 ThemeGroupingName
		//protected override void OnRegisterResourceSets()
		protected override void OnRegisterResourceSets(string groupingName)
		{
			// AS 11/6/07 ThemeGroupingName
			if (groupingName == EditorsGeneric.GroupingName || null == groupingName)
			{
				
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

				AssemblyResourceSetLoader.RegisterResourceSet(EditorsGeneric.Location);
				AssemblyResourceSetLoader.RegisterResourceSet(EditorsOnyx.Location);
				AssemblyResourceSetLoader.RegisterResourceSet(EditorsAero.Location);
				AssemblyResourceSetLoader.RegisterResourceSet(EditorsRoyale.Location);
				AssemblyResourceSetLoader.RegisterResourceSet(EditorsLunaNormal.Location);
				AssemblyResourceSetLoader.RegisterResourceSet(EditorsLunaOlive.Location);
				AssemblyResourceSetLoader.RegisterResourceSet(EditorsLunaSilver.Location);
				AssemblyResourceSetLoader.RegisterResourceSet(EditorsOffice2k7Black.Location);
				AssemblyResourceSetLoader.RegisterResourceSet(EditorsOffice2k7Blue.Location);
				AssemblyResourceSetLoader.RegisterResourceSet(EditorsOffice2k7Silver.Location);
                // AS 9/3/08 TFS6988
                // The washbase themes are base themes - they should not be registered.
                //
				//AssemblyResourceSetLoader.RegisterResourceSet(EditorsWashBaseDark.Location);
				//AssemblyResourceSetLoader.RegisterResourceSet(EditorsWashBaseLight.Location);

                // AS 10/6/08
                AssemblyResourceSetLoader.RegisterResourceSet(EditorsPrintBasic.Location);

				// JJD 8/30/10 - Added Office 2010 Blue theme
				AssemblyResourceSetLoader.RegisterResourceSet(EditorsOffice2010Blue.Location);

				// JJD 10/29/10 - Added IGTheme
				AssemblyResourceSetLoader.RegisterResourceSet(EditorsIGTheme.Location);

				// JJD 02/16/12 - Added Metro Theme
				AssemblyResourceSetLoader.RegisterResourceSet(EditorsMetro.Location);


			}
		}
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