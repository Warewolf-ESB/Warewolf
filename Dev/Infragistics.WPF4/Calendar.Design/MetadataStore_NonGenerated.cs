using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Windows.Design.Metadata;
using Microsoft.Windows.Design.PropertyEditing;
using System.Reflection;
using Infragistics.Controls.Editors;
using System.ComponentModel;
using Infragistics.Collections;
using Infragistics;
using System.Windows.Markup;
using Infragistics.Windows.Helpers;
using System.Globalization;




namespace InfragisticsWPF4.Controls.Editors.XamCalendar.Design

{

	internal partial class MetadataStore : IProvideAttributeTable
	{
		#region Private Members

		private int _loadsedAssemblyCount;

		private static string[] _inheritedUIPropsToHide =
		{
			"AllowDrop",
			"Background",
			"BindingGroup",
			"BorderBrush",
			"BorderThickness",
			"Clip",
			"ClipToBounds",
			"ContextMenu",
			"Cursor",
			"Effect",
			"FlowDirection",
			"Focusable",
			"FocusVisualStyle",
			"FontFamily",
			"FontSize",
			"FontStretch",
			"FontStyle",
			"FontWeight",
			"ForceCursor",
			"Foreground",
			"Height",
			"HorizontalAlignment",
			"HorizontalContentAlignment",
			"IsEnabled",
			"IsHitTestVisible",
			"IsManipulationEnabled",
			"IsTabStop",
			"LayoutTransform",
			"Margin",
			"MaxHeight",
			"MaxWidth",
			"MinHeight",
			"MinWidth",
			"Opacity",
			"OpacityMask",
			"OverridesDefaultStyle",
			"Padding",
			"Projection",
			"RenderTransform",
			"RenderTransformOrigin",
			"SnapsToDevicePixels",
			"TabIndex",
			"TabNavigation",
			"Template",
			"ToolTip",
			"Uid",
			"UseLayoutRounding",
			"VerticalAlignment",
			"VerticalContentAlignment",
			"Width",
			"Visibility",
			"ZIndex"
		};

		#endregion //Private Members	
    
		#region AddCustomAttributes

		private void AddCustomAttributes(AttributeTableBuilder builder)
		{

			// JJD 9/27/11 - TFS86621/TFS88451
			// Add nullable type converter attributes to supply values in property grid for both SL and WPF
			builder.AddCallback(typeof(Infragistics.Controls.Editors.XamCalendar), delegate(AttributeCallbackBuilder callbackBuilder)
			{
				callbackBuilder.AddCustomAttributes("FirstDayOfWeek", new TypeConverterAttribute(typeof(NullableConverter<DayOfWeek>)));
				callbackBuilder.AddCustomAttributes("WeekRule", new TypeConverterAttribute(typeof(NullableConverter<CalendarWeekRule>)));
			});
		}

		#endregion //AddCustomAttributes

		#region HideInheritedUIProps

		private void HideInheritedUIProps(AttributeTableBuilder builder, Type type)
		{
			foreach (string propName in _inheritedUIPropsToHide)
				builder.AddCustomAttributes(type, propName, new BrowsableAttribute(false));
		}

		#endregion //HideInheritedUIProps	
    
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