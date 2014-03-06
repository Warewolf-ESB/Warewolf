using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Globalization;
using System.ComponentModel;
using System.Windows;
using Infragistics.Collections;

namespace Infragistics.Controls.Schedules
{
	/// <summary>
	/// Represents the settings to use when connecting to the Microsoft Exchange Server. 
	/// </summary>

	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_ExchangeConnector, Version = FeatureInfo.Version_11_1)]

	public sealed class ExchangeServerConnectionSettings : DependencyObject
	{
		#region Member Variables

		private WeakList<ExchangeScheduleDataConnector> _owners; 

		#endregion  // Member Variables

		#region Constructor

		/// <summary>
		/// Initializes a new <see cref="ExchangeServerConnectionSettings"/> instance.
		/// </summary>
		public ExchangeServerConnectionSettings() { }

		#endregion // Constructor

		#region Methods

		#region AddOwner

		internal void AddOwner(ExchangeScheduleDataConnector owner)
		{
			if (_owners == null)
				_owners = new WeakList<ExchangeScheduleDataConnector>();

			_owners.Add(owner);
		} 

		#endregion  // AddOwner

		#region NotifyOwnersOfChange

		private void NotifyOwnersOfChange()
		{
			if (_owners == null)
				return;

			ExchangeScheduleDataConnector[] owningConnectors = _owners.ToArray();

			for (int i = 0; i < owningConnectors.Length; i++)
			{
				ExchangeScheduleDataConnector owningConnector = owningConnectors[i];

				if (owningConnector == null)
					continue;

				owningConnector.OnServerConnectionSettingsChanged();
			}
		} 

		#endregion  // NotifyOwnersOfChange

		#region OnPropertyChanged

		private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ExchangeServerConnectionSettings settings = (ExchangeServerConnectionSettings)d;
			settings.NotifyOwnersOfChange();
		}

		#endregion  // OnPropertyChanged

		#region RemoveOwner

		internal void RemoveOwner(ExchangeScheduleDataConnector owner)
		{
			if (_owners == null)
				return;

			_owners.Remove(owner);
			_owners.Compact();
		}

		#endregion  // RemoveOwner

		#endregion  // Methods

		#region Properties


		#region AcceptGZipEncoding

		/// <summary>
		/// Identifies the <see cref="AcceptGZipEncoding"/> dependency property
		/// </summary>
		public readonly static DependencyProperty AcceptGZipEncodingProperty = DependencyProperty.Register(
			"AcceptGZipEncoding",
			typeof(bool),
			typeof(ExchangeServerConnectionSettings),
			DependencyPropertyUtilities.CreateMetadata(true, OnPropertyChanged));

		/// <summary>
		/// Gets or sets the value indicating whether the server should return information compressed with GZip.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The server will only send Gzip compressed data if this value is True and the server is configured to send GZip compressed responses.
		/// </p>
		/// </remarks>
		/// <seealso cref="AcceptGZipEncodingProperty"/>
		public bool AcceptGZipEncoding
		{
			get { return (bool)this.GetValue(AcceptGZipEncodingProperty); }
			set { this.SetValue(AcceptGZipEncodingProperty, value); }
		}

		#endregion // AcceptGZipEncoding


		#region CookieContainer

		/// <summary>
		/// Identifies the <see cref="CookieContainer"/> dependency property
		/// </summary>
		public readonly static DependencyProperty CookieContainerProperty = DependencyProperty.Register(
			"CookieContainer",
			typeof(CookieContainer),
			typeof(ExchangeServerConnectionSettings),
			DependencyPropertyUtilities.CreateMetadata(null, OnPropertyChanged));

		/// <summary>
		/// Gets or sets the cookies associated with the server.
		/// </summary>
		/// <seealso cref="CookieContainerProperty"/>
		public CookieContainer CookieContainer
		{
			get { return (CookieContainer)this.GetValue(CookieContainerProperty); }
			set { this.SetValue(CookieContainerProperty, value); }
		}

		#endregion // CookieContainer

		#region HttpHeaders

		/// <summary>
		/// Identifies the <see cref="HttpHeaders"/> dependency property
		/// </summary>
		public readonly static DependencyProperty HttpHeadersProperty = DependencyProperty.Register(
			"HttpHeaders",
			typeof(Dictionary<string, string>),
			typeof(ExchangeServerConnectionSettings),
			DependencyPropertyUtilities.CreateMetadata(null, OnPropertyChanged));

		/// <summary>
		/// Gets or sets the HTTP headers which should be sent with each request to the server.
		/// </summary>
		/// <seealso cref="HttpHeadersProperty"/>
		public Dictionary<string, string> HttpHeaders
		{
			get { return (Dictionary<string, string>)this.GetValue(HttpHeadersProperty); }
			set { this.SetValue(HttpHeadersProperty, value); }
		}

		#endregion // HttpHeaders


		#region PreAuthenticate

		/// <summary>
		/// Identifies the <see cref="PreAuthenticate"/> dependency property
		/// </summary>
		public readonly static DependencyProperty PreAuthenticateProperty = DependencyProperty.Register(
			"PreAuthenticate",
			typeof(bool),
			typeof(ExchangeServerConnectionSettings),
			DependencyPropertyUtilities.CreateMetadata(false, OnPropertyChanged));

		/// <summary>
		/// Gets or sets the value indicating whether HTTP pre-authentication should be used when connecting to the server.
		/// </summary>
		/// <seealso cref="PreAuthenticateProperty"/>
		public bool PreAuthenticate
		{
			get { return (bool)this.GetValue(PreAuthenticateProperty); }
			set { this.SetValue(PreAuthenticateProperty, value); }
		}

		#endregion // PreAuthenticate


		#region RequestedServerVersion

		/// <summary>
		/// Identifies the <see cref="RequestedServerVersion"/> dependency property
		/// </summary>
		public readonly static DependencyProperty RequestedServerVersionProperty = DependencyProperty.Register(
			"RequestedServerVersion",
			typeof(ExchangeVersion),
			typeof(ExchangeServerConnectionSettings),
			// MD 4/27/11 - TFS72779
			//DependencyPropertyUtilities.CreateMetadata(ExchangeVersion.Exchange2007_SP1, OnPropertyChanged));
			DependencyPropertyUtilities.CreateMetadata(ExchangeVersion.AutoDetect, OnPropertyChanged));

		/// <summary>
		/// Gets or sets the requested version of the server.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		/// The value is set after the first connection to the server has been made.
		/// </exception>
		/// <seealso cref="RequestedServerVersionProperty"/>
		public ExchangeVersion RequestedServerVersion
		{
			get { return (ExchangeVersion)this.GetValue(RequestedServerVersionProperty); }
			set { this.SetValue(RequestedServerVersionProperty, value); }
		}

		#endregion // RequestedServerVersion

		#region Timeout

		/// <summary>
		/// Identifies the <see cref="Timeout"/> dependency property
		/// </summary>
		public readonly static DependencyProperty TimeoutProperty = DependencyProperty.Register(
			"Timeout",
			typeof(int),
			typeof(ExchangeServerConnectionSettings),
			DependencyPropertyUtilities.CreateMetadata(100000, OnPropertyChanged));

		/// <summary>
		/// Gets or sets the timeout used when requesting information from the server, in milliseconds.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If this is not set, a value of 100000 will be used.
		/// </p>
		/// </remarks>
		/// <seealso cref="TimeoutProperty"/>
		public int Timeout
		{
			get { return (int)this.GetValue(TimeoutProperty); }
			set { this.SetValue(TimeoutProperty, value); }
		}

		#endregion // Timeout

		#region Url

		/// <summary>
		/// Identifies the <see cref="Url"/> dependency property
		/// </summary>
		public readonly static DependencyProperty UrlProperty = DependencyProperty.Register(
			"Url",
			typeof(Uri),
			typeof(ExchangeServerConnectionSettings),
			DependencyPropertyUtilities.CreateMetadata(null, OnPropertyChanged));

		/// <summary>
		/// Gets or sets the URL to the Microsoft Exchange Server. 
		/// </summary>
		/// <seealso cref="UrlProperty"/>
		public Uri Url
		{
			get { return (Uri)this.GetValue(UrlProperty); }
			set { this.SetValue(UrlProperty, value); }
		}

		#endregion // Url


		#region UserAgent

		/// <summary>
		/// Identifies the <see cref="UserAgent"/> dependency property
		/// </summary>
		public readonly static DependencyProperty UserAgentProperty = DependencyProperty.Register(
			"UserAgent",
			typeof(string),
			typeof(ExchangeServerConnectionSettings),
			DependencyPropertyUtilities.CreateMetadata(null, OnPropertyChanged));

		/// <summary>
		/// Gets or sets the value of the user-agent HTTP header.
		/// </summary>
		/// <seealso cref="UserAgentProperty"/>
		public string UserAgent
		{
			get { return (string)this.GetValue(UserAgentProperty); }
			set { this.SetValue(UserAgentProperty, value); }
		}

		#endregion // UserAgent

		#region WebProxy

		/// <summary>
		/// Identifies the <see cref="WebProxy"/> dependency property
		/// </summary>
		public readonly static DependencyProperty WebProxyProperty = DependencyProperty.Register(
			"WebProxy",
			typeof(IWebProxy),
			typeof(ExchangeServerConnectionSettings),
			DependencyPropertyUtilities.CreateMetadata(null, OnPropertyChanged));

		/// <summary>
		/// Gets or sets the web proxy used when connecting to the server.
		/// </summary>
		/// <seealso cref="WebProxyProperty"/>
		public IWebProxy WebProxy
		{
			get { return (IWebProxy)this.GetValue(WebProxyProperty); }
			set { this.SetValue(WebProxyProperty, value); }
		}

		#endregion // WebProxy


		#endregion // Properties
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