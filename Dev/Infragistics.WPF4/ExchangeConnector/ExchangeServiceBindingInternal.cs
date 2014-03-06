using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Controls.Schedules.EWS;
using System.Net;
using System.Xml;






namespace Infragistics.Controls.Schedules
{
	internal class ExchangeServiceBindingInternal : ExchangeServiceBinding
	{
		// MD 10/21/11 - TFS87807
		// Keep track of whether we created at least one service binding.
		private static bool _hasAnyServiceBindingBeenCreated;

		// MD 4/27/11 - TFS72779
		private ExchangeService _service;

		public ExchangeServiceBindingInternal()
		{
			// MD 10/21/11 - TFS87807
			// Mark this flag once any binding is created.
			_hasAnyServiceBindingBeenCreated = true;
		}



#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

		// MD 4/27/11 - TFS72779
		protected override XmlReader GetReaderForMessage(System.Web.Services.Protocols.SoapClientMessage message, int bufferSize)
		{
			if (_service != null && _service.Connector.AutoDetectedServerVersion.HasValue == false)
			{
				using (XmlReader reader = XmlReader.Create(message.Stream))
					EWSUtilities.ResolveAutoDetectedVersion(_service, reader);

				message.Stream.Position = 0;
			}

			return base.GetReaderForMessage(message, bufferSize);
		}

		protected override WebRequest GetWebRequest(Uri uri)
		{
			WebRequest webRequest = base.GetWebRequest(uri);

			if (this.AcceptGzipEncoding)
			{
				webRequest.Headers[HttpRequestHeader.AcceptEncoding] = "gzip,deflate";
			}

			if (this.HttpHeaders != null && this.HttpHeaders.Count > 0)
			{
				foreach (KeyValuePair<string, string> pair in this.HttpHeaders)
					webRequest.Headers[pair.Key] = pair.Value;
			}

			return webRequest;
		}

		public bool AcceptGzipEncoding { get; set; }


		// MD 10/21/11 - TFS87807
		internal static bool HasAnyServiceBindingBeenCreated
		{
			get { return _hasAnyServiceBindingBeenCreated; }
		}

		public IDictionary<string, string> HttpHeaders { get; set; }

		// MD 4/27/11 - TFS72779
		public ExchangeService Service
		{
			get { return _service; }
			set 
			{
				if (_service == value)
					return;

				_service = value;




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