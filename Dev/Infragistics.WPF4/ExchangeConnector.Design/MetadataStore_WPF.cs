using System;
using System.ComponentModel;
using Microsoft.Windows.Design.Metadata;
using Microsoft.Windows.Design.PropertyEditing;
using System.Reflection;

[assembly: ProvideMetadata(typeof(InfragisticsWPF4.Controls.SchedulesExchangeConnector.Design.MetadataStore))]

namespace InfragisticsWPF4.Controls.SchedulesExchangeConnector.Design
{
	internal partial class MetadataStore : IProvideAttributeTable
	{
		public AttributeTable AttributeTable
		{
			get
			{
			    bool isVS = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName.Equals("devenv.exe"); 
				AttributeTableBuilder tableBuilder = new AttributeTableBuilder();
				Type t = typeof(Infragistics.Controls.Schedules.ExchangeScheduleDataConnector);
				Assembly controlAssembly = t.Assembly;

				#region ExchangeScheduleDataConnector Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ExchangeScheduleDataConnector");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("ExchangeScheduleDataConnectorAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("ExchangeScheduleDataConnectorAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "ResourceItems",
					new DescriptionAttribute(SR.GetString("ExchangeScheduleDataConnector_ResourceItems_Property")),
				    new DisplayNameAttribute("ResourceItems"),
					new CategoryAttribute(SR.GetString("ExchangeConnector_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsPolling",
					new DescriptionAttribute(SR.GetString("ExchangeScheduleDataConnector_IsPolling_Property")),
				    new DisplayNameAttribute("IsPolling"),
					new CategoryAttribute(SR.GetString("ExchangeConnector_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PollingInterval",
					new DescriptionAttribute(SR.GetString("ExchangeScheduleDataConnector_PollingInterval_Property")),
				    new DisplayNameAttribute("PollingInterval"),
					new CategoryAttribute(SR.GetString("ExchangeConnector_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ServerConnectionSettings",
					new DescriptionAttribute(SR.GetString("ExchangeScheduleDataConnector_ServerConnectionSettings_Property")),
				    new DisplayNameAttribute("ServerConnectionSettings"),
					new CategoryAttribute(SR.GetString("ExchangeConnector_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "UseDefaultCredentials",
					new DescriptionAttribute(SR.GetString("ExchangeScheduleDataConnector_UseDefaultCredentials_Property")),
				    new DisplayNameAttribute("UseDefaultCredentials"),
					new CategoryAttribute(SR.GetString("ExchangeConnector_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Users",
					new DescriptionAttribute(SR.GetString("ExchangeScheduleDataConnector_Users_Property")),
				    new DisplayNameAttribute("Users"),
					new CategoryAttribute(SR.GetString("ExchangeConnector_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "UseServerWorkingHours",
					new DescriptionAttribute(SR.GetString("ExchangeScheduleDataConnector_UseServerWorkingHours_Property")),
				    new DisplayNameAttribute("UseServerWorkingHours"),
					new CategoryAttribute(SR.GetString("ExchangeConnector_Properties"))
				);

				#endregion // ExchangeScheduleDataConnector Properties

				#region ExchangeUser Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ExchangeUser");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Domain",
					new DescriptionAttribute(SR.GetString("ExchangeUser_Domain_Property")),
				    new DisplayNameAttribute("Domain"),
					new CategoryAttribute(SR.GetString("ExchangeConnector_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Password",
					new DescriptionAttribute(SR.GetString("ExchangeUser_Password_Property")),
				    new DisplayNameAttribute("Password"),
					new CategoryAttribute(SR.GetString("ExchangeConnector_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "UserName",
					new DescriptionAttribute(SR.GetString("ExchangeUser_UserName_Property")),
				    new DisplayNameAttribute("UserName"),
					new CategoryAttribute(SR.GetString("ExchangeConnector_Properties"))
				);

				#endregion // ExchangeUser Properties

				#region ExchangeServerConnectionSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ExchangeServerConnectionSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AcceptGZipEncoding",
					new DescriptionAttribute(SR.GetString("ExchangeServerConnectionSettings_AcceptGZipEncoding_Property")),
				    new DisplayNameAttribute("AcceptGZipEncoding"),
					new CategoryAttribute(SR.GetString("ExchangeConnector_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CookieContainer",
					new DescriptionAttribute(SR.GetString("ExchangeServerConnectionSettings_CookieContainer_Property")),
				    new DisplayNameAttribute("CookieContainer"),
					new CategoryAttribute(SR.GetString("ExchangeConnector_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HttpHeaders",
					new DescriptionAttribute(SR.GetString("ExchangeServerConnectionSettings_HttpHeaders_Property")),
				    new DisplayNameAttribute("HttpHeaders"),
					new CategoryAttribute(SR.GetString("ExchangeConnector_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PreAuthenticate",
					new DescriptionAttribute(SR.GetString("ExchangeServerConnectionSettings_PreAuthenticate_Property")),
				    new DisplayNameAttribute("PreAuthenticate"),
					new CategoryAttribute(SR.GetString("ExchangeConnector_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "RequestedServerVersion",
					new DescriptionAttribute(SR.GetString("ExchangeServerConnectionSettings_RequestedServerVersion_Property")),
				    new DisplayNameAttribute("RequestedServerVersion"),
					new CategoryAttribute(SR.GetString("ExchangeConnector_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Timeout",
					new DescriptionAttribute(SR.GetString("ExchangeServerConnectionSettings_Timeout_Property")),
				    new DisplayNameAttribute("Timeout"),
					new CategoryAttribute(SR.GetString("ExchangeConnector_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Url",
					new DescriptionAttribute(SR.GetString("ExchangeServerConnectionSettings_Url_Property")),
				    new DisplayNameAttribute("Url"),
					new CategoryAttribute(SR.GetString("ExchangeConnector_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "UserAgent",
					new DescriptionAttribute(SR.GetString("ExchangeServerConnectionSettings_UserAgent_Property")),
				    new DisplayNameAttribute("UserAgent"),
					new CategoryAttribute(SR.GetString("ExchangeConnector_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "WebProxy",
					new DescriptionAttribute(SR.GetString("ExchangeServerConnectionSettings_WebProxy_Property")),
				    new DisplayNameAttribute("WebProxy"),
					new CategoryAttribute(SR.GetString("ExchangeConnector_Properties"))
				);

				#endregion // ExchangeServerConnectionSettings Properties
                this.AddCustomAttributes(tableBuilder);
				return tableBuilder.CreateTable();
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